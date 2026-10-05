using AI_ticket_analyzer.Models.DTO;
using System.Text.Json;

namespace AI_ticket_analyzer.Service
{
    public class GroqClient : IAiClient
    {
        private readonly HttpClient httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger<GroqClient> _logger;

        public GroqClient(HttpClient httpClient, IConfiguration config, ILogger<GroqClient> logger)
        {
            this.httpClient = httpClient;
            _apiKey = config["GroqApi:ApiKey"] ?? throw new ArgumentNullException(nameof(config), "ApiKey is not configured.");
            _model = config["GroqApi:Model"] ?? throw new ArgumentNullException(nameof(config), "Model is not configured.");
            _logger = logger;
        }

        public async Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest analyzeRequest)
        {
            try
            {
                var prompt = BuildPrompt(analyzeRequest.Title, analyzeRequest.Description);
                _logger.LogInformation("Sending ticket analysis request to Groq API.");

                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    model = _model,
                    temperature = 0.2,
                    max_completion_tokens = 1024,
                    top_p = 1,
                    response_format = new { type = "json_object" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

                request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                _logger.LogInformation("Received response from Groq API: {response}", response);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Received response from Groq API: {response}", errorText);
                    throw new InvalidOperationException($"Groq API HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {errorText}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var content = ExtractAssistantContent(json);
                var result = DeserializeAnalysisContent(content);

                _logger.LogInformation("Groq API response parsed successfully.");

                return result;
            }
            catch (InvalidAiResponseException ex)
            {
                _logger.LogWarning(ex, "Groq API returned an invalid response.");
                throw;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error analyzing ticket with Groq API.");
                throw;
            }
        }

        private static string ExtractAssistantContent(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("choices", out var choices) ||
                    choices.ValueKind != JsonValueKind.Array ||
                    choices.GetArrayLength() == 0)
                {
                    throw new InvalidAiResponseException("AI provider returned an invalid response.");
                }

                var firstChoice = choices[0];

                if (!firstChoice.TryGetProperty("message", out var message) ||
                    message.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidAiResponseException("AI provider returned an invalid response.");
                }

                if (!message.TryGetProperty("content", out var contentElement))
                {
                    throw new InvalidAiResponseException("AI provider returned an invalid response.");
                }

                var content = contentElement.GetString();

                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new InvalidAiResponseException("AI provider returned an empty response.");
                }

                return content;
            }
            catch (JsonException)
            {
                throw new InvalidAiResponseException("AI provider returned malformed JSON.");
            }
        }

        private static AnalyzeTicketResponse DeserializeAnalysisContent(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<AnalyzeTicketResponse>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? throw new InvalidAiResponseException("AI provider returned an invalid analysis.");
            }
            catch (JsonException)
            {
                throw new InvalidAiResponseException("AI provider returned malformed analysis JSON.");
            }
        }

        private string BuildPrompt(string title, string description)
        {

            var prompt = $$"""
                You are a support ticket classification system.

                Title: {{title}}
                Description: {{description}}

                Tasks:
                1. One-sentence summary
                2. Category (Billing, Login, Bug, Feature Request, Support, Other)
                3. Priority (Low, Medium, High)

                Return ONLY valid JSON:
                {
                  "summary": "",
                  "category": "",
                  "priority": ""
                }
                """;
             _logger.LogInformation("Prompt generated successfully");

            return prompt;
        }
    }
}

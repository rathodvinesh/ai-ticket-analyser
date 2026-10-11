using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AI_ticket_analyzer.Exceptions;
using AI_ticket_analyzer.Models.DTO;

namespace AI_ticket_analyzer.Service
{
    public class GroqClient : IAiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger<GroqClient> _logger;

        public GroqClient(HttpClient httpClient, IConfiguration config, ILogger<GroqClient> logger)
        {
            _httpClient = httpClient;
            _apiKey = config["GroqApi:ApiKey"] ?? string.Empty;
            _model = config["GroqApi:Model"] ?? "llama-3.3-70b-versatile";
            _logger = logger;
        }

        public async Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest analyzeRequest, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Groq API key is not configured. Set GroqApi:ApiKey via environment variables or user secrets.");
            }

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

                using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                _logger.LogInformation("Received HTTP {StatusCode} response from Groq API.", (int)response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var statusCode = (int)response.StatusCode;
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Groq API returned failure status code {StatusCode} ({ReasonPhrase}): {ErrorBody}", statusCode, response.ReasonPhrase, errorBody);
                    throw new AiProviderException($"Groq API HTTP {statusCode} ({response.ReasonPhrase}): {errorBody}", statusCode);
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var content = ExtractAssistantContent(json);
                var result = DeserializeAnalysisContent(content);

                _logger.LogInformation("Groq API response parsed successfully.");
                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Request to Groq API was cancelled by caller.");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to Groq API timed out.");
                throw new TimeoutException("Request to Groq API timed out.", ex);
            }
            catch (InvalidAiResponseException ex)
            {
                _logger.LogWarning(ex, "Groq API returned an invalid response.");
                throw;
            }
            catch (AiProviderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while analyzing ticket with Groq API.");
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

            _logger.LogInformation("Prompt generated successfully.");
            return prompt;
        }
    }
}

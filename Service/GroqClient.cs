using AI_ticket_analyzer.Models.DTO;
using System.Text.Json;

namespace AI_ticket_analyzer.Service
{
    public class GroqClient : IAiClient
    {
        private readonly HttpClient httpClient;
        private readonly string _apiKey;
        private readonly string _baseuri;
        private readonly string _model;
        private readonly ILogger<GroqClient> _logger;

        public GroqClient(HttpClient httpClient, IConfiguration config, ILogger<GroqClient> logger)
        {
            this.httpClient = httpClient;
            _apiKey = config["GroqApi:ApiKey"] ?? throw new ArgumentNullException(nameof(config), "ApiKey is not configured.");
            _baseuri = config["GroqApi:BaseUrl"]?? throw new ArgumentNullException(nameof(config), "BaseUrl is not configured.");
            _model = config["GroqApi:Model"] ?? throw new ArgumentNullException(nameof(config), "Model is not configured.");
            _logger = logger;
        }

        public async Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest analyzeRequest)
        {
            if (httpClient.BaseAddress == null)
            {
                httpClient.BaseAddress = new Uri(string.IsNullOrEmpty(_baseuri) ? "https://api.groq.com/openai/v1/" : _baseuri);
            }

            var result = new AnalyzeTicketResponse();
            try
            {
                var prompt = BuildPrompt(analyzeRequest.Title, analyzeRequest.Description);
                _logger.LogInformation("Sending prompt to Groq API: {Prompt}", prompt);

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

                var fullUri = new Uri(new Uri(string.IsNullOrWhiteSpace(_baseuri) ? "https://api.groq.com/openai/v1/" : _baseuri), "chat/completions");
                var request = new HttpRequestMessage(HttpMethod.Post, fullUri);
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

                using var doc = JsonDocument.Parse(json);

                var content = doc.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();

                result = JsonSerializer.Deserialize<AnalyzeTicketResponse>(
                    content!,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new AnalyzeTicketResponse();

                _logger.LogInformation("Send response from groq to ai API: {response}", response);

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error analyzing ticket with Groq API.");
                throw;
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

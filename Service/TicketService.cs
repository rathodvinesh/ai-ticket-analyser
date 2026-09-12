using AI_ticket_analyzer.Models;
using System.Text.Json;

namespace AI_ticket_analyzer.Service
{
    public class TicketService : ITicketService
    {
        private readonly HttpClient httpClient;
        private readonly string _apiKey;
        private readonly string _baseuri;
        private readonly string _model;
        public TicketService(HttpClient _httpClient, IConfiguration config)
        {
            httpClient = _httpClient;
            _apiKey = config["GroqApi:ApiKey"] ?? throw new ArgumentNullException(nameof(config), "ApiKey is not configured.");
            _baseuri = string.IsNullOrWhiteSpace(config["GroqApi:BaseUrl"]) ? "https://api.groq.com/openai/v1/" : config["GroqApi:BaseUrl"]!;
            _model = string.IsNullOrWhiteSpace(config["GroqApi:Model"]) ? "groq/compound-mini" : config["GroqApi:Model"]!;
        }

        public async Task<TicketResponse> AnalyzeTicketAsync(string title, string description)
        {
            if (httpClient.BaseAddress == null)
            {
                httpClient.BaseAddress = new Uri(string.IsNullOrEmpty(_baseuri) ? "https://api.groq.com/openai/v1/" : _baseuri);
            }

            var prompt = BuildPrompt(title, description);

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
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Groq API HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {errorText}");
            }

            var json = await response.Content.ReadAsStringAsync();

            //var ticketResponse = System.Text.Json.JsonSerializer.Deserialize<TicketResponse>(json, new System.Text.Json.JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true
            //});

            //return ticketResponse ?? new TicketResponse();

            using var doc = JsonDocument.Parse(json);
            //var content = doc.RootElement
            //    .GetProperty("priority")[0]
            //    .GetProperty("category")
            //    .GetProperty("summary")
            //    .GetString();

            var content = doc.RootElement
           .GetProperty("choices")[0]
           .GetProperty("message")
           .GetProperty("content")
           .GetString();

            return JsonSerializer.Deserialize<TicketResponse>(
                content!,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new TicketResponse();
        }

        private string BuildPrompt(string title, string description)
        {
            var prompt = $$"""
                You are a support ticket classification system.

                Title: {{title}}
                Description: {{description}}

                Tasks:
                1. One-sentence summary
                2. Category (Billing, Login, Bug, Feature Request, Other)
                3. Priority (Low, Medium, High)

                Return ONLY valid JSON:
                {
                  "summary": "",
                  "category": "",
                  "priority": ""
                }
                """;
            return prompt;
        }
    }
}

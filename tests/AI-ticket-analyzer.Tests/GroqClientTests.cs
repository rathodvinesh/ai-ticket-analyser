using System.Net;
using System.Text;
using System.Text.Json;
using AI_ticket_analyzer.Exceptions;
using AI_ticket_analyzer.Models.DTO;
using AI_ticket_analyzer.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace AI_ticket_analyzer.Tests
{
    public class GroqClientTests
    {
        private readonly Mock<ILogger<GroqClient>> _mockLogger = new();

        private IConfiguration CreateConfiguration(string apiKey = "test-key", string model = "llama-3.3-70b-versatile")
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "GroqApi:ApiKey", apiKey },
                { "GroqApi:Model", model }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        private HttpClient CreateMockHttpClient(HttpResponseMessage response)
        {
            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            return new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://api.groq.com/openai/v1/")
            };
        }

        [Fact]
        public async Task AnalyzeTicketAsync_ThrowsInvalidOperationException_WhenApiKeyIsMissing()
        {
            var config = CreateConfiguration(apiKey: "");
            var httpClient = new HttpClient { BaseAddress = new Uri("https://api.groq.com/openai/v1/") };
            var client = new GroqClient(httpClient, config, _mockLogger.Object);

            var request = new AnalyzeTicketRequest { Title = "Test", Description = "Test" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => client.AnalyzeTicketAsync(request));
        }

        [Fact]
        public async Task AnalyzeTicketAsync_ReturnsParsedResponse_WhenGroqReturnsValidJson()
        {
            var groqPayload = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            role = "assistant",
                            content = JsonSerializer.Serialize(new
                            {
                                summary = "Database connection pool exhausted",
                                category = "Bug",
                                priority = "High"
                            })
                        }
                    }
                }
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(groqPayload), Encoding.UTF8, "application/json")
            };

            var httpClient = CreateMockHttpClient(httpResponse);
            var client = new GroqClient(httpClient, CreateConfiguration(), _mockLogger.Object);

            var request = new AnalyzeTicketRequest { Title = "DB Down", Description = "App crashing on start" };
            var result = await client.AnalyzeTicketAsync(request);

            Assert.Equal("Database connection pool exhausted", result.Summary);
            Assert.Equal("Bug", result.Category);
            Assert.Equal("High", result.Priority);
        }

        [Theory]
        [InlineData(HttpStatusCode.TooManyRequests, 429)]
        [InlineData(HttpStatusCode.InternalServerError, 500)]
        [InlineData(HttpStatusCode.ServiceUnavailable, 503)]
        public async Task AnalyzeTicketAsync_ThrowsAiProviderException_WhenUpstreamFails(HttpStatusCode statusCode, int expectedCode)
        {
            var httpResponse = new HttpResponseMessage(statusCode)
            {
                ReasonPhrase = "Upstream failure",
                Content = new StringContent("{\"error\":\"rate_limit_exceeded\"}")
            };

            var httpClient = CreateMockHttpClient(httpResponse);
            var client = new GroqClient(httpClient, CreateConfiguration(), _mockLogger.Object);

            var request = new AnalyzeTicketRequest { Title = "Test", Description = "Test" };
            var ex = await Assert.ThrowsAsync<AiProviderException>(() => client.AnalyzeTicketAsync(request));

            Assert.Equal(expectedCode, ex.StatusCode);
        }

        [Fact]
        public async Task AnalyzeTicketAsync_ThrowsInvalidAiResponseException_WhenMalformedJsonReturned()
        {
            var groqPayload = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            role = "assistant",
                            content = "this is not valid json content"
                        }
                    }
                }
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(groqPayload), Encoding.UTF8, "application/json")
            };

            var httpClient = CreateMockHttpClient(httpResponse);
            var client = new GroqClient(httpClient, CreateConfiguration(), _mockLogger.Object);

            var request = new AnalyzeTicketRequest { Title = "Test", Description = "Test" };
            await Assert.ThrowsAsync<InvalidAiResponseException>(() => client.AnalyzeTicketAsync(request));
        }

        [Fact]
        public async Task AnalyzeTicketAsync_ThrowsOperationCanceledException_WhenCancellationRequested()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new OperationCanceledException());

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://api.groq.com/openai/v1/")
            };

            var client = new GroqClient(httpClient, CreateConfiguration(), _mockLogger.Object);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var request = new AnalyzeTicketRequest { Title = "Test", Description = "Test" };
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.AnalyzeTicketAsync(request, cts.Token));
        }
    }
}

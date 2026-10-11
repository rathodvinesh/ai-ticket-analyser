using System.IO;
using System.Text.Json;
using AI_ticket_analyzer.Exceptions;
using AI_ticket_analyzer.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AI_ticket_analyzer.Tests
{
    public class GlobalExceptionHandlerMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _mockLogger = new();
        private readonly Mock<IHostEnvironment> _mockEnvironment = new();

        public GlobalExceptionHandlerMiddlewareTests()
        {
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        }

        private async Task<(int StatusCode, ProblemDetails Details)> ExecuteMiddlewareAsync(Exception exceptionToThrow)
        {
            var middleware = new GlobalExceptionHandlerMiddleware(
                next: (innerHttpContext) => throw exceptionToThrow,
                logger: _mockLogger.Object,
                environment: _mockEnvironment.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = "/api/ticket/analyze";
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseText = await reader.ReadToEndAsync();
            var details = JsonSerializer.Deserialize<ProblemDetails>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

            return (context.Response.StatusCode, details);
        }

        [Fact]
        public async Task InvokeAsync_InvalidAiResponseException_Returns502()
        {
            var (statusCode, details) = await ExecuteMiddlewareAsync(new InvalidAiResponseException("Malformed response"));

            Assert.Equal(StatusCodes.Status502BadGateway, statusCode);
            Assert.Equal(StatusCodes.Status502BadGateway, details.Status);
            Assert.Equal("AI Provider Error", details.Title);
        }

        [Fact]
        public async Task InvokeAsync_TimeoutException_Returns504()
        {
            var (statusCode, details) = await ExecuteMiddlewareAsync(new TimeoutException("Upstream timed out"));

            Assert.Equal(StatusCodes.Status504GatewayTimeout, statusCode);
            Assert.Equal(StatusCodes.Status504GatewayTimeout, details.Status);
            Assert.Equal("Gateway Timeout", details.Title);
        }

        [Fact]
        public async Task InvokeAsync_AiProviderException_With429_Returns429()
        {
            var (statusCode, details) = await ExecuteMiddlewareAsync(new AiProviderException("Rate limit hit", 429));

            Assert.Equal(StatusCodes.Status429TooManyRequests, statusCode);
            Assert.Equal(StatusCodes.Status429TooManyRequests, details.Status);
        }

        [Fact]
        public async Task InvokeAsync_AiProviderException_With500_Returns502()
        {
            var (statusCode, details) = await ExecuteMiddlewareAsync(new AiProviderException("Internal provider failure", 500));

            Assert.Equal(StatusCodes.Status502BadGateway, statusCode);
            Assert.Equal(StatusCodes.Status502BadGateway, details.Status);
        }

        [Fact]
        public async Task InvokeAsync_GenericException_Returns500()
        {
            var (statusCode, details) = await ExecuteMiddlewareAsync(new InvalidOperationException("Something unexpected broke"));

            Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
            Assert.Equal(StatusCodes.Status500InternalServerError, details.Status);
            Assert.Equal("Internal Server Error", details.Title);
        }
    }
}

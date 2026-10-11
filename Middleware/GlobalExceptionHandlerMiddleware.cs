using System.Net;
using System.Text.Json;
using AI_ticket_analyzer.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AI_ticket_analyzer.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Request was cancelled by the client: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning(
                        "The response has already started. Exception handling cannot replace it.");
                    throw;
                }

                _logger.LogError(
                    ex,
                    "Unhandled exception occurred while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var (statusCode, title, detail) = exception switch
            {
                AiProviderException providerEx => (
                    providerEx.StatusCode == StatusCodes.Status429TooManyRequests
                        ? StatusCodes.Status429TooManyRequests
                        : StatusCodes.Status502BadGateway,
                    "AI Provider Error",
                    _environment.IsDevelopment()
                        ? providerEx.Message
                        : (providerEx.StatusCode == StatusCodes.Status429TooManyRequests
                            ? "AI provider rate limit exceeded. Please retry later."
                            : "The AI service encountered an upstream error.")
                ),
                InvalidAiResponseException aiEx => (
                    StatusCodes.Status502BadGateway,
                    "AI Provider Error",
                    "The AI service could not process the request."
                ),
                TimeoutException timeoutEx => (
                    StatusCodes.Status504GatewayTimeout,
                    "Gateway Timeout",
                    "The upstream AI service took too long to respond."
                ),
                BadHttpRequestException badHttpEx => (
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    badHttpEx.Message
                ),
                KeyNotFoundException notFoundEx => (
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    notFoundEx.Message
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred while processing your request."
                )
            };

            context.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            if (_environment.IsDevelopment())
            {
                problemDetails.Extensions["exception"] = exception.GetType().Name;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            // Compatibility field for clients that inspect 'message'
            problemDetails.Extensions["message"] = detail;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
        }
    }

    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}

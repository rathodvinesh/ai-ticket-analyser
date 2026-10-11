using AI_ticket_analyzer.Data;
using AI_ticket_analyzer.Middleware;
using AI_ticket_analyzer.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AITicketAnalyzerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbString"));
});

var useMockAi = builder.Configuration.GetValue<bool>("UseMockAi");

// Always register TicketService as ITicketService so domain logic & persistence remain consistent
builder.Services.AddScoped<ITicketService, TicketService>();

// Register AI Client based on configuration
if (useMockAi)
{
    builder.Services.AddScoped<IAiClient, MockAiClient>();
}
else
{
    builder.Services.AddHttpClient<IAiClient, GroqClient>(client =>
    {
        var baseUrl = builder.Configuration["GroqApi:BaseUrl"] ?? "https://api.groq.com/openai/v1/";
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddStandardResilienceHandler();
}

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 1. Global Exception Handling Middleware (must be first in pipeline)
app.UseGlobalExceptionHandler();

// 2. Swagger documentation for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. CORS
app.UseCors("AllowAll");

// 4. Static Files
app.UseDefaultFiles();
app.UseStaticFiles();

// 5. Security & Routing
app.UseHttpsRedirection();
app.UseAuthorization();

// 6. Endpoints
app.MapHealthChecks("/health");
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }

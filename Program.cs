using AI_ticket_analyzer.Data;
using AI_ticket_analyzer.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AITicketAnalyzerDbContext>(options =>
{
   options.UseSqlServer(builder.Configuration.GetConnectionString("DbString"));
});

var useMockAi = builder.Configuration.GetValue<bool>("UseMockAi");

// Register TicketService always


    // Use a lightweight in-memory AI client for local/dev
if (useMockAi) { 
    builder.Services.AddScoped<ITicketService, MockTicketAiService>();
}
else
{
    builder.Services.AddScoped<ITicketService, TicketService>();
    // Register the HTTP-based Groq client as the IAiClient implementation
    builder.Services.AddHttpClient<IAiClient, GroqClient>(client =>
    {
        var baseUrl = builder.Configuration["GroqApi:BaseUrl"] ?? "https://api.groq.com/openai/v1/";
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        client.Timeout = TimeSpan.FromSeconds(30); // Set a timeout for the HTTP requests
    });
}


// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature =
            context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

        var exception = exceptionFeature?.Error;

        context.Response.ContentType = "application/problem+json";

        if (exception is TimeoutException)
        {
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;

            await context.Response.WriteAsJsonAsync(new
            {
                status = 504,
                title = "Gateway Timeout",
                detail = "The AI service took too long to respond."
            });
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                status = 500,
                title = "Internal Server Error",
                detail = "An unexpected error occurred."
            });
        }
    });
});

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

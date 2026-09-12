using AI_ticket_analyzer.Data;
using AI_ticket_analyzer.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AITicketAnalyzerDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("DbString") ?? string.Empty;
    if (connStr.Contains("postgres", StringComparison.OrdinalIgnoreCase) ||
        connStr.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
        connStr.Contains("sslmode", StringComparison.OrdinalIgnoreCase))
    {
        var formattedConnStr = ConvertPostgresUrlToConnectionString(connStr);
        options.UseNpgsql(formattedConnStr);
    }
    else
    {
        options.UseSqlServer(connStr);
    }
});

var useMockAi = builder.Configuration.GetValue<bool>("UseMockAi");

if (useMockAi)
{
    builder.Services.AddScoped<ITicketService, MockTicketAiService>();
}
else
{
    builder.Services.AddHttpClient<ITicketService, TicketService>(client =>
    {
        var baseUrl = builder.Configuration["GroqApi:BaseUrl"] ?? "https://api.groq.com/openai/v1/";
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
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

app.MapControllers();
app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AITicketAnalyzerDbContext>();
        var dbCreator = db.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
        if (dbCreator != null)
        {
            if (!dbCreator.Exists())
            {
                dbCreator.Create();
            }
            if (!dbCreator.HasTables())
            {
                dbCreator.CreateTables();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB Auto-Init Note: {ex.Message}");
    }
}

app.Run();

static string ConvertPostgresUrlToConnectionString(string url)
{
    if (url.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        url.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Postgres URL parse warning: {ex.Message}");
        }
    }
    return url;
}

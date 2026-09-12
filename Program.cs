using AI_ticket_analyzer.Data;
using AI_ticket_analyzer.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AITicketAnalyzerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbString"));
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

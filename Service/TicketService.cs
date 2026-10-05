using AI_ticket_analyzer.Data;
using AI_ticket_analyzer.Models;
using AI_ticket_analyzer.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace AI_ticket_analyzer.Service
{
    public class TicketService : ITicketService
    {
        private static readonly string[] AllowedCategories =
        {
            "Billing",
            "Login",
            "Bug",
            "Feature Request",
            "Support",
            "Other"
        };

        private static readonly string[] AllowedPriorities =
        {
            "Low",
            "Medium",
            "High"
        };

        private readonly IAiClient _aiClient;
        private readonly AITicketAnalyzerDbContext _dbContext;
        private readonly ILogger<TicketService> _logger;

        public TicketService(IAiClient aiClient, AITicketAnalyzerDbContext dbContext, ILogger<TicketService> logger)
        {
            _aiClient = aiClient;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<TicketHistoryResponse>> GetRecentTicketsAsync()
        {
            return await _dbContext.SupportTickets
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .Select(t => new TicketHistoryResponse
                {
                    Title = t.RawTitle,
                    Description = t.RawDescription,
                    Summary = t.Aisummary ?? string.Empty,
                    Category = t.Aicategory ?? string.Empty,
                    Priority = t.Aipriority ?? string.Empty,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest analyzeRequest)
        {
            var response = await _aiClient.AnalyzeTicketAsync(analyzeRequest);

            ValidateAiResponse(response);

            var ticket = new SupportTicket
            {
                RawTitle = analyzeRequest.Title,
                RawDescription = analyzeRequest.Description,
                Aisummary = response.Summary,
                Aicategory = response.Category,
                Aipriority = response.Priority,
                CreatedAt = DateTime.UtcNow,
                AiprocessedAt = DateTime.UtcNow
            };

            _dbContext.SupportTickets.Add(ticket);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Analyzed ticket saved to database");
                
            return response;

        }

        private static void ValidateAiResponse(AnalyzeTicketResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.Summary))
            {
                throw new InvalidAiResponseException("AI response is invalid: summary is required.");
            }

            if (response.Summary.Length > 500)
            {
                throw new InvalidAiResponseException("AI response is invalid: summary is too long.");
            }

            if (string.IsNullOrWhiteSpace(response.Category))
            {
                throw new InvalidAiResponseException("AI response is invalid: category is required.");
            }

            if (!AllowedCategories.Contains(response.Category))
            {
                throw new InvalidAiResponseException("AI response is invalid: category is not supported.");
            }

            if (string.IsNullOrWhiteSpace(response.Priority))
            {
                throw new InvalidAiResponseException("AI response is invalid: priority is required.");
            }

            if (!AllowedPriorities.Contains(response.Priority))
            {
                throw new InvalidAiResponseException("AI response is invalid: priority is not supported.");
            }
        }
    }
}

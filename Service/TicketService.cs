using AI_ticket_analyzer.Data;
using AI_ticket_analyzer.Models;
using AI_ticket_analyzer.Models.DTO;
using System.Text.Json;

namespace AI_ticket_analyzer.Service
{
    public class TicketService : ITicketService
    {
        private readonly IAiClient _aiClient;
        private readonly AITicketAnalyzerDbContext _dbContext;
        private readonly ILogger<TicketService> _logger;

        public TicketService(IAiClient aiClient, AITicketAnalyzerDbContext dbContext, ILogger<TicketService> logger)
        {
            _aiClient = aiClient;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest analyzeRequest)
        {

            var allowedCategories = new List<string> { "Billing", "Login", "Bug", "Feature Request", "Support", "Other" };

            var allowedPriorities = new List<string> { "Low", "Medium", "High" };

            var response = await _aiClient.AnalyzeTicketAsync(analyzeRequest);

            if (!allowedCategories.Contains(response.Category, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Invalid category returned by AI: {response.Category}. Allowed categories are: {string.Join(", ", allowedCategories)}");
            }

            if (!allowedPriorities.Contains(response.Priority, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Invalid priority returned by AI: {response.Priority}. Allowed priorities are: {string.Join(", ", allowedPriorities)}");
            }

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

      
    }
}

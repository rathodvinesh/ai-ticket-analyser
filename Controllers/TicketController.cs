using AI_ticket_analyzer.Data;
using AI_ticket_analyzer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI_ticket_analyzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly Service.ITicketService _groqService;
        private readonly AITicketAnalyzerDbContext _dbContext;

        public TicketController(Service.ITicketService groqService, AITicketAnalyzerDbContext dbContext)
        {
            _groqService = groqService;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            try
            {
                var tickets = await _dbContext.SupportTickets
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(50)
                    .ToListAsync();

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching tickets from database: {ex.Message}");
            }
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeTicket([FromBody] TicketRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest("Title and Description are required.");
            }

            var result = await _groqService.AnalyzeTicketAsync(request.Title, request.Description);

            // Save analyzed ticket to SQL Database
            try
            {
                var ticket = new SupportTicket
                {
                    RawTitle = request.Title,
                    RawDescription = request.Description,
                    Aisummary = result.Summary,
                    Aicategory = result.Category,
                    Aipriority = result.Priority,
                    CreatedAt = DateTime.UtcNow,
                    AiprocessedAt = DateTime.UtcNow
                };

                _dbContext.SupportTickets.Add(ticket);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log DB save error without failing AI result response
                Console.WriteLine($"DB Save Warning: {ex.Message}");
            }

            return Ok(result);
        }
    }
}

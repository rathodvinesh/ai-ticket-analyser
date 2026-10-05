using AI_ticket_analyzer.Data;
using AI_ticket_analyzer.Models;
using AI_ticket_analyzer.Models.DTO;
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
        public async Task<IActionResult> AnalyzeTicket([FromBody] AnalyzeTicketRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest("Title and Description are required.");
            }

            if(request.Title.Length > 200)
            {
                return BadRequest("Title cannot exceed 200 characters.");
            }

            if(request.Description.Length > 5000)
            {
                return BadRequest("Description cannot exceed 5000 characters.");
            }

            var result = await _groqService.AnalyzeTicketAsync(request);

            return Ok(result);
            
        }
    }
}

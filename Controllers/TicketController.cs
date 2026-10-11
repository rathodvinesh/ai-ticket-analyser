using AI_ticket_analyzer.Models.DTO;
using AI_ticket_analyzer.Service;
using Microsoft.AspNetCore.Mvc;

namespace AI_ticket_analyzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets(CancellationToken cancellationToken)
        {
            var tickets = await _ticketService.GetRecentTicketsAsync(cancellationToken);
            return Ok(tickets);
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeTicket([FromBody] AnalyzeTicketRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest("Title and Description are required.");
            }

            if (request.Title.Length > 200)
            {
                return BadRequest("Title cannot exceed 200 characters.");
            }

            if (request.Description.Length > 5000)
            {
                return BadRequest("Description cannot exceed 5000 characters.");
            }

            var result = await _ticketService.AnalyzeTicketAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}

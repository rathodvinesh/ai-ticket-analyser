using AI_ticket_analyzer.Models.DTO;
using AI_ticket_analyzer.Service;
using Microsoft.AspNetCore.Mvc;

namespace AI_ticket_analyzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly Service.ITicketService _ticketService;

        public TicketController(Service.ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets()
        {
            var tickets = await _ticketService.GetRecentTicketsAsync();

            return Ok(tickets);
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

            try
            {
                var result = await _ticketService.AnalyzeTicketAsync(request);

                return Ok(result);
            }
            catch (InvalidAiResponseException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new
                    {
                        message = ex.Message
                    });
            }
            
        }
    }
}

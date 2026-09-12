using AI_ticket_analyzer.Models;

namespace AI_ticket_analyzer.Service
{
    public interface ITicketService
    {
        Task<TicketResponse> AnalyzeTicketAsync(string title,string description);
    }
}

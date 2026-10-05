using AI_ticket_analyzer.Models;
using AI_ticket_analyzer.Models.DTO;

namespace AI_ticket_analyzer.Service
{
    public interface ITicketService
    {
        Task<IReadOnlyList<TicketHistoryResponse>> GetRecentTicketsAsync();
        Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest request);
    }
}

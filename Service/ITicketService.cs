using AI_ticket_analyzer.Models.DTO;

namespace AI_ticket_analyzer.Service
{
    public interface ITicketService
    {
        Task<IReadOnlyList<TicketHistoryResponse>> GetRecentTicketsAsync(CancellationToken cancellationToken = default);
        Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest request, CancellationToken cancellationToken = default);
    }
}

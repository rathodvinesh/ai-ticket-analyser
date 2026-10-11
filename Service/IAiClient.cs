using AI_ticket_analyzer.Models.DTO;

namespace AI_ticket_analyzer.Service
{
    public interface IAiClient
    {
        Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest request, CancellationToken cancellationToken = default);
    }
}

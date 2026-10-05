using AI_ticket_analyzer.Models.DTO;

namespace AI_ticket_analyzer.Service
{
    public interface IAiClient
    {
        public Task<AnalyzeTicketResponse> AnalyzeTicketAsync(AnalyzeTicketRequest request);
    }
}

namespace AI_ticket_analyzer.Models.DTO
{
    public sealed class AnalyzeTicketRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}

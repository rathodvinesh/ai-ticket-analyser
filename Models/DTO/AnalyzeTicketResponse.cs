namespace AI_ticket_analyzer.Models.DTO
{
    public sealed class AnalyzeTicketResponse
    {
        public string Summary { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
    }
}

namespace AI_ticket_analyzer.Models.DTO
{
    public sealed class TicketHistoryResponse
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Summary { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}

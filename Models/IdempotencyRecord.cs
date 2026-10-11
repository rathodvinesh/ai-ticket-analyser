namespace AI_ticket_analyzer.Models
{
    public class IdempotencyRecord
    {
        public int Id { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
        public string RequestHash { get; set; } = string.Empty;
        public string ResponseData { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

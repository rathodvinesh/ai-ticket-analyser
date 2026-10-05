namespace AI_ticket_analyzer.Models.DTO
{
    public sealed class ServiceResponse<T>
    {
        public bool Success { get; set; } = false;
        public string? Error { get; set; }
        public string? RawContent { get; set; }
        public T? Data { get; set; }
    }
}

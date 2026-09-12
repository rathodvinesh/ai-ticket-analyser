using System;
using System.Collections.Generic;

namespace AI_ticket_analyzer.Models;

public partial class SupportTicket
{
    public int TicketId { get; set; }

    public string RawTitle { get; set; } = null!;

    public string RawDescription { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? Aisummary { get; set; }

    public string? Aicategory { get; set; }

    public string? Aipriority { get; set; }

    public DateTime? AiprocessedAt { get; set; }
}

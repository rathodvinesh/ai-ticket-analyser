using System;
using System.Collections.Generic;
using AI_ticket_analyzer.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_ticket_analyzer.Data;

public partial class AITicketAnalyzerDbContext : DbContext
{
    public AITicketAnalyzerDbContext()
    {
    }

    public AITicketAnalyzerDbContext(DbContextOptions<AITicketAnalyzerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SupportTicket> SupportTickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__SupportT__712CC607CDE7AE19");

            entity.Property(e => e.Aicategory)
                .HasMaxLength(50)
                .HasColumnName("AICategory");
            entity.Property(e => e.Aipriority)
                .HasMaxLength(20)
                .HasColumnName("AIPriority");
            entity.Property(e => e.AiprocessedAt).HasColumnName("AIProcessedAt");
            entity.Property(e => e.Aisummary)
                .HasMaxLength(500)
                .HasColumnName("AISummary");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.RawTitle).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

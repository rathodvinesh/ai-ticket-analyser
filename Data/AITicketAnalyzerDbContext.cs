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
    public virtual DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK_SupportTickets");

            entity.Property(e => e.RawTitle)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.RawDescription)
                .IsRequired();

            entity.Property(e => e.Aicategory)
                .HasMaxLength(50)
                .HasColumnName("AICategory");

            entity.Property(e => e.Aipriority)
                .HasMaxLength(20)
                .HasColumnName("AIPriority");

            entity.Property(e => e.AiprocessedAt)
                .HasColumnName("AIProcessedAt");

            entity.Property(e => e.Aisummary)
                .HasMaxLength(500)
                .HasColumnName("AISummary");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Optimizes GetRecentTicketsAsync ORDER BY CreatedAt DESC
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_SupportTickets_CreatedAt");

            // Optimizes category filtering queries
            entity.HasIndex(e => new { e.Aicategory, e.CreatedAt })
                .HasDatabaseName("IX_SupportTickets_AICategory_CreatedAt");
        });

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(e => e.RequestHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(32)
                .IsRequired();

            entity.HasIndex(e => e.IdempotencyKey)
                .IsUnique()
                .HasDatabaseName("IX_IdempotencyRecords_Key");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

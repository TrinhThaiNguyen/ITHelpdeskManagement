using ITHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHelpdesk.Infrastructure.Persistence.Configurations;

public class TicketHistoryConfiguration
    : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.ToTable("TicketHistories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.OldValue)
            .HasMaxLength(1000);

        builder.Property(h => h.NewValue)
            .HasMaxLength(1000);

        builder.Property(h => h.ChangedBy)
            .HasMaxLength(256);

        builder.HasOne(h => h.Ticket)
            .WithMany(t => t.Histories)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => new { h.TicketId, h.CreatedAt });
    }
}
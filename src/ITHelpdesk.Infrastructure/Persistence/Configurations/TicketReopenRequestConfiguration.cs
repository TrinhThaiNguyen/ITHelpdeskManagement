
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHelpdesk.Infrastructure.Persistence.Configurations;

public class TicketReopenRequestConfiguration
    : IEntityTypeConfiguration<TicketReopenRequest>
{
    public void Configure(
        EntityTypeBuilder<TicketReopenRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.ReviewReason)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(x => x.Ticket)
            .WithMany()
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TicketId)
            .IsUnique()
            .HasFilter("[Status] = 1");
    }
}

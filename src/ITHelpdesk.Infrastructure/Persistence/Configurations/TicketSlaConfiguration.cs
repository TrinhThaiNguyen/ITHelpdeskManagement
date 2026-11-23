
using ITHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHelpdesk.Infrastructure.Persistence.Configurations;

public class TicketSlaConfiguration : IEntityTypeConfiguration<TicketSla>
{
    public void Configure(EntityTypeBuilder<TicketSla> builder)
    {
        builder.ToTable("TicketSlas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ResponseDeadline)
            .IsRequired();

        builder.Property(x => x.ResolutionDeadline)
            .IsRequired();

        builder.HasIndex(x => x.TicketId)
            .IsUnique();

        builder.HasOne(x => x.Ticket)
            .WithOne(x => x.TicketSla)
            .HasForeignKey<TicketSla>(x => x.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SlaPolicy)
            .WithMany(x => x.TicketSlas)
            .HasForeignKey(x => x.SlaPolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

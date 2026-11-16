using ITHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHelpdesk.Infrastructure.Persistence.Configurations;

public class TicketCommentConfiguration
    : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("TicketComments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(c => c.IsInternal)
            .IsRequired();

        builder.HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.TicketId, c.CreatedAt });
    }
}
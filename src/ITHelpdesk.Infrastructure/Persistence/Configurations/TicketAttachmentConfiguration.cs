using ITHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHelpdesk.Infrastructure.Persistence.Configurations;

public class TicketAttachmentConfiguration
    : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.ToTable("TicketAttachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.FileSizeBytes)
            .IsRequired();

        builder.HasOne(a => a.Ticket)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.TicketId);
    }
}
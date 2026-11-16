using ITHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHelpdesk.Infrastructure.Persistence.Configurations;

public class TicketCategoryConfiguration
    : IEntityTypeConfiguration<TicketCategory>
{
    public void Configure(EntityTypeBuilder<TicketCategory> builder)
    {
        builder.ToTable("TicketCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.HasIndex(c => c.Name)
            .IsUnique();
    }
}
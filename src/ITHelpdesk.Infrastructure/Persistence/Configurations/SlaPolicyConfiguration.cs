
using ITHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHelpdesk.Infrastructure.Persistence.Configurations;

public class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.ToTable("SlaPolicies", table =>
        {
            table.HasCheckConstraint(
                "CK_SlaPolicies_ResponseTimeMinutes",
                "[ResponseTimeMinutes] > 0");

            table.HasCheckConstraint(
                "CK_SlaPolicies_ResolutionTimeMinutes",
                "[ResolutionTimeMinutes] > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.ResponseTimeMinutes)
            .IsRequired();

        builder.Property(x => x.ResolutionTimeMinutes)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // M?i Priority ch? có m?t Policy ?ang ho?t ??ng.
        builder.HasIndex(x => x.Priority)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.HasMany(x => x.TicketSlas)
            .WithOne(x => x.SlaPolicy)
            .HasForeignKey(x => x.SlaPolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

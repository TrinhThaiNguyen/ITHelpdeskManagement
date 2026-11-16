using ITHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHelpdesk.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(4000);

        // Indexes
        builder.HasIndex(t => t.Status);

        builder.HasIndex(t => t.Priority);

        builder.HasIndex(t => t.CreatedAt);

        // Ticket - Category
        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket - Department
        builder.HasOne(t => t.Department)
            .WithMany(d => d.Tickets)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket - Requester
        builder.HasOne(t => t.Requester)
            .WithMany(e => e.RequestedTickets)
            .HasForeignKey(t => t.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket - Assignee
        builder.HasOne(t => t.Assignee)
            .WithMany(e => e.AssignedTickets)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
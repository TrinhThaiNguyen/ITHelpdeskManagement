
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Infrastructure.Identity;
using ITHelpdesk.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Persistence;

public class AppDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();

    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();

    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne<Employee>()
                .WithOne()
                .HasForeignKey<ApplicationUser>(user => user.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(user => user.EmployeeId)
                .IsUnique()
                .HasFilter("[EmployeeId] IS NOT NULL");
        });
    }
}

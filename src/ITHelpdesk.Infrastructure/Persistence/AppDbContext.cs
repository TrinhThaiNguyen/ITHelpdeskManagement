using ITHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ITHelpdesk.Infrastructure.Persistence.Configurations;

namespace ITHelpdesk.Infrastructure.Persistence;

public class AppDbContext : DbContext
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
    }
}
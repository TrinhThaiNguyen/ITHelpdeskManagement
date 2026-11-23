
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Domain.Enums;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Persistence.Seeders;

public static class SlaPolicySeeder
{
    private static readonly List<SlaPolicy> DefaultPolicies =
    [
        new SlaPolicy
        {
            Name = "Low SLA",
            Priority = TicketPriority.Low,
            ResponseTimeMinutes = 480,
            ResolutionTimeMinutes = 4320
        },
        new SlaPolicy
        {
            Name = "Medium SLA",
            Priority = TicketPriority.Medium,
            ResponseTimeMinutes = 240,
            ResolutionTimeMinutes = 1440
        },
        new SlaPolicy
        {
            Name = "High SLA",
            Priority = TicketPriority.High,
            ResponseTimeMinutes = 60,
            ResolutionTimeMinutes = 480
        },
        new SlaPolicy
        {
            Name = "Critical SLA",
            Priority = TicketPriority.Critical,
            ResponseTimeMinutes = 15,
            ResolutionTimeMinutes = 240
        }
    ];

    public static async Task SeedAsync(
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        foreach (var defaultPolicy in DefaultPolicies)
        {
            var exists = await context.SlaPolicies
                .AnyAsync(
                    x => x.Priority == defaultPolicy.Priority
                         && x.IsActive,
                    cancellationToken);

            if (!exists)
            {
                context.SlaPolicies.Add(defaultPolicy);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

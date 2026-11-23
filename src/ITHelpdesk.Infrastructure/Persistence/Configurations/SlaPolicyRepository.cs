using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Domain.Enums;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Persistence.Repositories;

public class SlaPolicyRepository : ISlaPolicyRepository
{
    private readonly AppDbContext _context;

    public SlaPolicyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SlaPolicy?> GetActiveByPriorityAsync(
        TicketPriority priority,
        CancellationToken cancellationToken = default)
    {
        return await _context.SlaPolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Priority == priority && x.IsActive,
                cancellationToken);
    }
}
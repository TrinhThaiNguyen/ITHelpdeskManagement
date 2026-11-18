
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class TicketHistoryRepository : ITicketHistoryRepository
{
    private readonly AppDbContext _context;

    public TicketHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        TicketHistory history,
        CancellationToken cancellationToken = default)
    {
        await _context.TicketHistories.AddAsync(
            history,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TicketHistory>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TicketHistories
            .AsNoTracking()
            .Where(h => h.TicketId == ticketId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

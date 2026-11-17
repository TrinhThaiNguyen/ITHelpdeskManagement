
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Infrastructure.Persistence;

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
}

using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Department)
            .FirstOrDefaultAsync(
                t => t.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Department)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default)
    {
        await _context.Tickets.AddAsync(ticket, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
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

    public Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Tickets
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Department)
            .FirstOrDefaultAsync(
                t => t.Id == id && !t.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
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

    public Task<Ticket?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Department)
            .FirstOrDefaultAsync(
                t => t.Id == id && !t.IsDeleted,
                cancellationToken);
    }

    public async Task UpdateAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default)
    {
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default)
    {
        ticket.IsDeleted = true;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
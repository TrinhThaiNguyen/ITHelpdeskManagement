
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class TicketCommentRepository : ITicketCommentRepository
{
    private readonly AppDbContext _context;

    public TicketCommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        TicketComment comment,
        CancellationToken cancellationToken = default)
    {
        await _context.TicketComments.AddAsync(
            comment,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TicketComment>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TicketComments
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

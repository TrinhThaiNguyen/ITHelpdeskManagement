
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class TicketAttachmentRepository : ITicketAttachmentRepository
{
    private readonly AppDbContext _context;

    public TicketAttachmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        TicketAttachment attachment,
        CancellationToken cancellationToken = default)
    {
        await _context.TicketAttachments.AddAsync(
            attachment,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TicketAttachment>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TicketAttachments
            .AsNoTracking()
            .Where(a => a.TicketId == ticketId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TicketAttachment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.TicketAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == id,
                cancellationToken);
    }

    public async Task DeleteAsync(
        TicketAttachment attachment,
        CancellationToken cancellationToken = default)
    {
        _context.TicketAttachments.Remove(attachment);

        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

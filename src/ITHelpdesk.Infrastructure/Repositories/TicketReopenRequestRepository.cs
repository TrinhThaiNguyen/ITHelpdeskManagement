
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Persistence.Repositories;

public class TicketReopenRequestRepository
    : ITicketReopenRequestRepository
{
    private readonly AppDbContext _context;

    public TicketReopenRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TicketReopenRequest?> GetByIdAsync(Guid id)
    {
        return await _context.TicketReopenRequests
            .Include(x => x.Ticket)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> HasPendingRequestAsync(Guid ticketId)
    {
        return await _context.TicketReopenRequests
            .AnyAsync(x =>
                x.TicketId == ticketId &&
                x.Status == Domain.Enums.TicketReopenRequestStatus.Pending);
    }

    public async Task<List<TicketReopenRequest>> GetByTicketIdAsync(
        Guid ticketId)
    {
        return await _context.TicketReopenRequests
            .Where(x => x.TicketId == ticketId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(TicketReopenRequest request)
    {
        await _context.TicketReopenRequests.AddAsync(request);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

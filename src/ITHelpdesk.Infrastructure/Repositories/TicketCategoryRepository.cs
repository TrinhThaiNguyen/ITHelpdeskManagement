using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class TicketCategoryRepository : ITicketCategoryRepository
{
    private readonly AppDbContext _context;

    public TicketCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TicketCategory?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.TicketCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<TicketCategory>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.TicketCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
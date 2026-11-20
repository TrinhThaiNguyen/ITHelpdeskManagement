
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

    public async Task<TicketCategory?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.TicketCategories
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

    public async Task<IReadOnlyList<TicketCategory>> GetAllForManagementAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.TicketCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.TicketCategories.AnyAsync(
            c => c.Name == name &&
                 (!excludeCategoryId.HasValue ||
                  c.Id != excludeCategoryId.Value),
            cancellationToken);
    }

    public async Task AddAsync(
        TicketCategory category,
        CancellationToken cancellationToken = default)
    {
        await _context.TicketCategories.AddAsync(
            category,
            cancellationToken);
    }

    public Task UpdateAsync(
        TicketCategory category,
        CancellationToken cancellationToken = default)
    {
        _context.TicketCategories.Update(category);
        return Task.CompletedTask;
    }

    public async Task<bool> HasTicketsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tickets.AnyAsync(
            t => t.CategoryId == categoryId,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

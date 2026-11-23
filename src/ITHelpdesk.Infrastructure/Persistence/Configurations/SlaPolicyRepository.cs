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

    public async Task<List<SlaPolicy>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SlaPolicies
            .AsNoTracking()
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<SlaPolicy?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.SlaPolicies
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExistsActivePolicyAsync(
        TicketPriority priority,
        Guid? excludePolicyId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.SlaPolicies
            .AnyAsync(
                x => x.Priority == priority
                     && x.IsActive
                     && (excludePolicyId == null
                         || x.Id != excludePolicyId.Value),
                cancellationToken);
    }

    public async Task AddAsync(
        SlaPolicy policy,
        CancellationToken cancellationToken = default)
    {
        await _context.SlaPolicies.AddAsync(
            policy,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        SlaPolicy policy,
        CancellationToken cancellationToken = default)
    {
        _context.SlaPolicies.Update(policy);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
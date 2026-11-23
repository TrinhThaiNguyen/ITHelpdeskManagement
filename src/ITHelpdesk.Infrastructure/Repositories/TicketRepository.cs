using ITHelpdesk.Application.DTOs.Pagination;
using ITHelpdesk.Application.DTOs.Tickets;
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
            .Include(t => t.TicketSla)
                .ThenInclude(ts => ts!.SlaPolicy)
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
            .Include(t => t.TicketSla)
                .ThenInclude(ts => ts!.SlaPolicy)
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
            .Include(t => t.TicketSla)
                .ThenInclude(ts => ts!.SlaPolicy)
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

    public Task<Ticket?> GetByIdAndRequesterIdAsync(
        Guid id,
        Guid requesterId,
        CancellationToken cancellationToken = default)
    {
        return _context.Tickets
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Department)
            .Include(t => t.TicketSla)
                .ThenInclude(ts => ts!.SlaPolicy)
            .FirstOrDefaultAsync(
                t => t.Id == id
                     && t.RequesterId == requesterId
                     && !t.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetByRequesterIdAsync(
        Guid requesterId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.RequesterId == requesterId && !t.IsDeleted)
            .Include(t => t.Category)
            .Include(t => t.Department)
            .Include(t => t.TicketSla)
                .ThenInclude(ts => ts!.SlaPolicy)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResponse<Ticket>> GetPagedAsync(
    TicketQueryParameters parameters,
    Guid? requesterId = null,
    CancellationToken cancellationToken = default)
    {
        // B?t ??u truy v?n Ticket ch?a b? xóa
        IQueryable<Ticket> query = _context.Tickets
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        // Requester ch? ???c xem Ticket c?a chính mình
        if (requesterId.HasValue)
        {
            query = query.Where(t => t.RequesterId == requesterId.Value);
        }

        // Search theo Title và Description
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.Trim();

            query = query.Where(t =>
                t.Title.Contains(searchTerm) ||
                t.Description.Contains(searchTerm));
        }

        // Filter theo Status
        if (parameters.Status.HasValue)
        {
            query = query.Where(t => t.Status == parameters.Status.Value);
        }

        // Filter theo Priority
        if (parameters.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == parameters.Priority.Value);
        }

        // Filter theo Department
        if (parameters.DepartmentId.HasValue)
        {
            query = query.Where(t =>
                t.DepartmentId == parameters.DepartmentId.Value);
        }

        // Filter theo Assignee
        if (parameters.AssigneeId.HasValue)
        {
            query = query.Where(t =>
                t.AssigneeId == parameters.AssigneeId.Value);
        }

        // ??m t?ng s? Ticket sau khi l?c
        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        var sortBy = (parameters.SortBy ?? "createdAt")
            .Trim()
            .ToLowerInvariant();

        IOrderedQueryable<Ticket> orderedQuery =
            (sortBy, parameters.SortDescending) switch
            {
                ("title", true) => query.OrderByDescending(t => t.Title),
                ("title", false) => query.OrderBy(t => t.Title),

                ("priority", true) => query.OrderByDescending(t => t.Priority),
                ("priority", false) => query.OrderBy(t => t.Priority),

                ("status", true) => query.OrderByDescending(t => t.Status),
                ("status", false) => query.OrderBy(t => t.Status),

                ("updatedat", true) => query.OrderByDescending(t => t.UpdatedAt),
                ("updatedat", false) => query.OrderBy(t => t.UpdatedAt),

                ("createdat", true) => query.OrderByDescending(t => t.CreatedAt),
                ("createdat", false) => query.OrderBy(t => t.CreatedAt),

                _ => query.OrderByDescending(t => t.CreatedAt)
            };

        // ??m b?o th? t? ?n ??nh khi có nhi?u Ticket cùng giá tr?
        orderedQuery = orderedQuery.ThenBy(t => t.Id);

        // Pagination và lay du lieu
        var items = await orderedQuery
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Include(t => t.Category)
            .Include(t => t.Department)
            .Include(t => t.TicketSla)
                .ThenInclude(ts => ts!.SlaPolicy)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Ticket>
        {
            Items = items,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }
}

using ITHelpdesk.Application.DTOs.Pagination;
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Domain.Enums;
using ITHelpdesk.Infrastructure.Persistence;
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
                x.Status == TicketReopenRequestStatus.Pending);
    }

    public async Task<List<TicketReopenRequest>> GetByTicketIdAsync(
        Guid ticketId)
    {
        return await _context.TicketReopenRequests
            .Where(x => x.TicketId == ticketId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<PagedResponse<TicketReopenRequestListResponse>>
        GetPagedAsync(TicketReopenRequestQueryParameters parameters)
    {
        var query = _context.TicketReopenRequests
            .AsNoTracking()
            .Include(x => x.Ticket)
                .ThenInclude(x => x.Requester)
            .AsQueryable();

        // Filtering
        if (parameters.Status.HasValue)
        {
            query = query.Where(x =>
                x.Status == parameters.Status.Value);
        }

        // Search
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.Trim();

            query = query.Where(x =>
                x.Ticket.Title.Contains(searchTerm) ||
                x.Ticket.Requester.FullName.Contains(searchTerm) ||
                x.Ticket.Requester.Email.Contains(searchTerm));
        }

        // Sorting
        query = parameters.SortBy.ToLower() switch
        {
            "createdat" => parameters.SortDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),

            "reviewedat" => parameters.SortDescending
                ? query.OrderByDescending(x => x.ReviewedAt)
                : query.OrderBy(x => x.ReviewedAt),

            "status" => parameters.SortDescending
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),

            "tickettitle" => parameters.SortDescending
                ? query.OrderByDescending(x => x.Ticket.Title)
                : query.OrderBy(x => x.Ticket.Title),

            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        // Pagination
        var pageNumber = Math.Max(1, parameters.PageNumber);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TicketReopenRequestListResponse
            {
                Id = x.Id,
                TicketId = x.TicketId,
                TicketTitle = x.Ticket.Title,
                TicketStatus = x.Ticket.Status,
                RequestedByUserId = x.RequestedByUserId,
                RequesterName = x.Ticket.Requester.FullName,
                RequesterEmail = x.Ticket.Requester.Email,
                Reason = x.Reason,
                Status = x.Status,
                ReviewedByUserId = x.ReviewedByUserId,
                ReviewReason = x.ReviewReason,
                CreatedAt = x.CreatedAt,
                ReviewedAt = x.ReviewedAt
            })
            .ToListAsync();

        return new PagedResponse<TicketReopenRequestListResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
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

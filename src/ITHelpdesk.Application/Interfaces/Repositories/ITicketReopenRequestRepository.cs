
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Application.DTOs.Pagination;
using ITHelpdesk.Application.DTOs.Tickets;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketReopenRequestRepository
{
    Task<TicketReopenRequest?> GetByIdAsync(Guid id);

    Task<bool> HasPendingRequestAsync(Guid ticketId);

    Task<List<TicketReopenRequest>> GetByTicketIdAsync(Guid ticketId);

    Task AddAsync(TicketReopenRequest request);

    Task<PagedResponse<TicketReopenRequestListResponse>> GetPagedAsync(
    TicketReopenRequestQueryParameters parameters);

    Task SaveChangesAsync();

}

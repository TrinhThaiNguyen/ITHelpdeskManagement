
using ITHelpdesk.Application.DTOs.Pagination;
using ITHelpdesk.Application.DTOs.Tickets;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface ITicketReopenRequestService
{
    Task<TicketReopenRequestResponse> CreateAsync(
        Guid ticketId,
        Guid requesterUserId,
        Guid requesterEmployeeId,
        CreateTicketReopenRequest request);

    Task<TicketReopenRequestResponse> ReviewAsync(
        Guid requestId,
        Guid reviewerUserId,
        ReviewTicketReopenRequest request);

    Task<List<TicketReopenRequestResponse>> GetByTicketIdAsync(
        Guid ticketId,
        Guid? requesterEmployeeId = null);

    Task<PagedResponse<TicketReopenRequestListResponse>> GetPagedAsync(
        TicketReopenRequestQueryParameters parameters);
}

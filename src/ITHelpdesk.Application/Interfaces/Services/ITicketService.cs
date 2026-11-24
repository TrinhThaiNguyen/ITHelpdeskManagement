using ITHelpdesk.Application.DTOs.Pagination;
using ITHelpdesk.Application.DTOs.Tickets;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface ITicketService
{
    Task<Guid> CreateTicketAsync(
        CreateTicketRequest request,
        Guid requesterId,
        CancellationToken cancellationToken = default);

    Task<TicketResponse?> GetTicketByIdAsync(
        Guid id,
        Guid? requesterId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketResponse>> GetAllTicketsAsync(
        Guid? requesterId = null,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<TicketResponse>> GetPagedTicketsAsync(
        TicketQueryParameters parameters,
        Guid? requesterId = null,
        CancellationToken cancellationToken = default);

    Task<TicketResponse?> UpdateTicketAsync(
        Guid id,
        UpdateTicketRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteTicketAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateTicketStatusAsync(
        Guid id,
        UpdateTicketStatusRequest request,
        string changedBy,
        Guid changedByUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketHistoryResponse>> GetTicketHistoryAsync(
        Guid ticketId,
        Guid? requesterId = null,
        CancellationToken cancellationToken = default);
}
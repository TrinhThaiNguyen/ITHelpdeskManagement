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
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketResponse>> GetAllTicketsAsync(
        CancellationToken cancellationToken = default);

    Task<TicketResponse?> UpdateTicketAsync(
        Guid id,
        UpdateTicketRequest request,
        CancellationToken cancellationToken = default);
}
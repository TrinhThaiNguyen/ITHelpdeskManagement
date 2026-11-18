using ITHelpdesk.Application.DTOs.Tickets;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface ITicketAssignmentService
{
    Task AssignAsync(
        Guid ticketId,
        AssignTicketRequest request,
        CancellationToken cancellationToken = default);

    Task UnassignAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<TicketAssignmentResponse> GetAssignmentAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);
}
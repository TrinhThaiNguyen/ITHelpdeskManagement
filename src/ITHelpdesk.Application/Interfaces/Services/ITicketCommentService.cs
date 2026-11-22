using ITHelpdesk.Application.DTOs.Tickets;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface ITicketCommentService
{
    Task<Guid> AddCommentAsync(
        Guid ticketId,
        CreateTicketCommentRequest request,
        Guid? requesterId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketCommentResponse>> GetCommentsAsync(
        Guid ticketId,
        Guid? requesterId = null,
        CancellationToken cancellationToken = default);
}
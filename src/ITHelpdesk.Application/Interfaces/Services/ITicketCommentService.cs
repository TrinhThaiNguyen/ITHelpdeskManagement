
using ITHelpdesk.Application.DTOs.Tickets;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface ITicketCommentService
{
    Task<Guid> AddCommentAsync(
        Guid ticketId,
        CreateTicketCommentRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketCommentResponse>> GetCommentsAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);
}

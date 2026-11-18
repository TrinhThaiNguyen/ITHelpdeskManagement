
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketCommentRepository
{
    Task AddAsync(
        TicketComment comment,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketComment>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}


using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketAttachmentRepository
{
    Task AddAsync(
        TicketAttachment attachment,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketAttachment>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<TicketAttachment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        TicketAttachment attachment,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

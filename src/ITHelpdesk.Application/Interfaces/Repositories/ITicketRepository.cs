using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default);

    Task<Ticket?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default);
}
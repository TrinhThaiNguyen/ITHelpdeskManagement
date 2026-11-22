using ITHelpdesk.Application.DTOs.Pagination;
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Ticket?> GetByIdAndRequesterIdAsync(
        Guid id,
        Guid requesterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetByRequesterIdAsync(
        Guid requesterId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<Ticket>> GetPagedAsync(
        TicketQueryParameters parameters,
        Guid? requesterId = null,
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
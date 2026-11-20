
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketCategoryRepository
{
    Task<TicketCategory?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<TicketCategory?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketCategory>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketCategory>> GetAllForManagementAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeCategoryId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TicketCategory category,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TicketCategory category,
        CancellationToken cancellationToken = default);

    Task<bool> HasTicketsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

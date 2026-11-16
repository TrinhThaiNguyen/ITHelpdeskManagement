using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketCategoryRepository
{
    Task<TicketCategory?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketCategory>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
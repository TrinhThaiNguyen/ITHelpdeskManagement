using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ISlaPolicyRepository
{
    // Existing method: used by TicketService
    Task<SlaPolicy?> GetActiveByPriorityAsync(
        TicketPriority priority,
        CancellationToken cancellationToken = default);

    // CRUD methods
    Task<List<SlaPolicy>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SlaPolicy?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    // Business validation
    Task<bool> ExistsActivePolicyAsync(
        TicketPriority priority,
        Guid? excludePolicyId = null,
        CancellationToken cancellationToken = default);

    // Create and update
    Task AddAsync(
        SlaPolicy policy,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        SlaPolicy policy,
        CancellationToken cancellationToken = default);
}
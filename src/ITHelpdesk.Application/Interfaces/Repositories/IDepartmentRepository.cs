using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Department>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
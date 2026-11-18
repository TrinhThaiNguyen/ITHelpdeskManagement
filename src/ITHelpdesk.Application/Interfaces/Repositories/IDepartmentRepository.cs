using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Department?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Department>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Department>> GetAllForManagementAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeDepartmentId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveEmployeesAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Department department,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Department department,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
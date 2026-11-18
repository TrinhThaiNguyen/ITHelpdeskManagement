using ITHelpdesk.Application.DTOs.Departments;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DepartmentResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
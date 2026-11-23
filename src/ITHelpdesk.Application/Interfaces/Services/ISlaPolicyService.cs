using ITHelpdesk.Application.DTOs.SlaPolicies;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface ISlaPolicyService
{
    Task<List<SlaPolicyResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SlaPolicyResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SlaPolicyResponse> CreateAsync(
        CreateSlaPolicyRequest request,
        CancellationToken cancellationToken = default);

    Task<SlaPolicyResponse?> UpdateAsync(
        Guid id,
        UpdateSlaPolicyRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default);
}
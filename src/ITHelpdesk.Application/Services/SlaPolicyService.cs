using ITHelpdesk.Application.DTOs.SlaPolicies;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Services;

public class SlaPolicyService : ISlaPolicyService
{
    private readonly ISlaPolicyRepository _slaPolicyRepository;

    public SlaPolicyService(
        ISlaPolicyRepository slaPolicyRepository)
    {
        _slaPolicyRepository = slaPolicyRepository;
    }

    public async Task<List<SlaPolicyResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var policies = await _slaPolicyRepository.GetAllAsync(
            cancellationToken);

        return policies.Select(MapToResponse).ToList();
    }

    public async Task<SlaPolicyResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var policy = await _slaPolicyRepository.GetByIdAsync(
            id,
            cancellationToken);

        return policy == null ? null : MapToResponse(policy);
    }

    public async Task<SlaPolicyResponse> CreateAsync(
        CreateSlaPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ResolutionTimeMinutes < request.ResponseTimeMinutes)
        {
            throw new ArgumentException(
                "Resolution time must be greater than or equal to response time.");
        }

        var exists = await _slaPolicyRepository.ExistsActivePolicyAsync(
            request.Priority,
            cancellationToken: cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "An active SLA policy already exists for this priority.");
        }

        var policy = new SlaPolicy
        {
            Name = request.Name.Trim(),
            Priority = request.Priority,
            ResponseTimeMinutes = request.ResponseTimeMinutes,
            ResolutionTimeMinutes = request.ResolutionTimeMinutes,
            IsActive = true
        };

        await _slaPolicyRepository.AddAsync(
            policy,
            cancellationToken);

        return MapToResponse(policy);
    }

    public async Task<SlaPolicyResponse?> UpdateAsync(
        Guid id,
        UpdateSlaPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        var policy = await _slaPolicyRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (policy == null)
        {
            return null;
        }

        if (request.ResolutionTimeMinutes < request.ResponseTimeMinutes)
        {
            throw new ArgumentException(
                "Resolution time must be greater than or equal to response time.");
        }

        if (policy.IsActive)
        {
            var exists = await _slaPolicyRepository.ExistsActivePolicyAsync(
                request.Priority,
                id,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException(
                    "An active SLA policy already exists for this priority.");
            }
        }

        policy.Name = request.Name.Trim();
        policy.Priority = request.Priority;
        policy.ResponseTimeMinutes = request.ResponseTimeMinutes;
        policy.ResolutionTimeMinutes = request.ResolutionTimeMinutes;
        policy.UpdatedAt = DateTime.UtcNow;

        await _slaPolicyRepository.UpdateAsync(
            policy,
            cancellationToken);

        return MapToResponse(policy);
    }

    public async Task<bool> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var policy = await _slaPolicyRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (policy == null)
        {
            return false;
        }

        if (policy.IsActive == isActive)
        {
            return true;
        }

        if (isActive)
        {
            var exists = await _slaPolicyRepository.ExistsActivePolicyAsync(
                policy.Priority,
                id,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException(
                    "An active SLA policy already exists for this priority.");
            }
        }

        policy.IsActive = isActive;
        policy.UpdatedAt = DateTime.UtcNow;

        await _slaPolicyRepository.UpdateAsync(
            policy,
            cancellationToken);

        return true;
    }

    private static SlaPolicyResponse MapToResponse(SlaPolicy policy)
    {
        return new SlaPolicyResponse
        {
            Id = policy.Id,
            Name = policy.Name,
            Priority = policy.Priority,
            ResponseTimeMinutes = policy.ResponseTimeMinutes,
            ResolutionTimeMinutes = policy.ResolutionTimeMinutes,
            IsActive = policy.IsActive,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }
}
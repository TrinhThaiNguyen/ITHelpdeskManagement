
using ITHelpdesk.Application.DTOs.Departments;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository
            .GetAllForManagementAsync(cancellationToken);

        return departments.Select(MapToResponse).ToList();
    }

    public async Task<DepartmentResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository
            .GetByIdAsync(id, cancellationToken);

        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        return MapToResponse(department);
    }

    public async Task<Guid> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = NormalizeName(request.Name);

        if (await _departmentRepository.ExistsByNameAsync(
            name, cancellationToken: cancellationToken))
        {
            throw new ArgumentException("Department name already exists.");
        }

        var department = new Department
        {
            Name = name,
            Description = NormalizeDescription(request.Description),
            IsActive = true
        };

        await _departmentRepository.AddAsync(department, cancellationToken);
        await _departmentRepository.SaveChangesAsync(cancellationToken);

        return department.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository
            .GetByIdForUpdateAsync(id, cancellationToken);

        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        var name = NormalizeName(request.Name);

        if (await _departmentRepository.ExistsByNameAsync(
            name, id, cancellationToken))
        {
            throw new ArgumentException("Department name already exists.");
        }

        department.Name = name;
        department.Description = NormalizeDescription(request.Description);
        department.UpdatedAt = DateTime.UtcNow;

        await _departmentRepository.UpdateAsync(department, cancellationToken);
        await _departmentRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository
            .GetByIdForUpdateAsync(id, cancellationToken);

        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        if (!department.IsActive)
            return;

        var hasActiveEmployees = await _departmentRepository
            .HasActiveEmployeesAsync(id, cancellationToken);

        if (hasActiveEmployees)
        {
            throw new InvalidOperationException(
                "Cannot deactivate a department that has active employees.");
        }

        department.IsActive = false;
        department.UpdatedAt = DateTime.UtcNow;

        await _departmentRepository.UpdateAsync(department, cancellationToken);
        await _departmentRepository.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeName(string name)
    {
        return name.Trim();
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    private static DepartmentResponse MapToResponse(Department department)
    {
        return new DepartmentResponse
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt
        };
    }
}

using ITHelpdesk.Application.DTOs.Employees;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);

        return employees.Select(MapToResponse).ToList();
    }

    public async Task<EmployeeResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(
            id, cancellationToken);

        if (employee == null)
            throw new KeyNotFoundException("Employee not found.");

        return MapToResponse(employee);
    }

    public async Task<Guid> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        var emailExists = await _employeeRepository.ExistsByEmailAsync(
            email,
            cancellationToken: cancellationToken);

        if (emailExists)
            throw new ArgumentException("Email already exists.");

        var departmentExists = await _departmentRepository.ExistsActiveAsync(
            request.DepartmentId, cancellationToken);

        if (!departmentExists)
            throw new ArgumentException("Department does not exist or is inactive.");

        var employee = new Employee
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber?.Trim(),
            DepartmentId = request.DepartmentId,
            IsActive = true
        };

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdForUpdateAsync(
            id, cancellationToken);

        if (employee == null)
            throw new KeyNotFoundException("Employee not found.");

        var email = NormalizeEmail(request.Email);

        var emailExists = await _employeeRepository.ExistsByEmailAsync(
            email,
            id,
            cancellationToken);

        if (emailExists)
            throw new ArgumentException("Email already exists.");

        var departmentExists = await _departmentRepository.ExistsActiveAsync(
            request.DepartmentId, cancellationToken);

        if (!departmentExists)
            throw new ArgumentException("Department does not exist or is inactive.");

        employee.FullName = request.FullName.Trim();
        employee.Email = email;
        employee.PhoneNumber = request.PhoneNumber?.Trim();
        employee.DepartmentId = request.DepartmentId;
        employee.UpdatedAt = DateTime.UtcNow;

        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdForUpdateAsync(
            id, cancellationToken);

        if (employee == null)
            throw new KeyNotFoundException("Employee not found.");

        if (!employee.IsActive)
            return;

        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;

        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static EmployeeResponse MapToResponse(Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department.Name,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        };
    }
}
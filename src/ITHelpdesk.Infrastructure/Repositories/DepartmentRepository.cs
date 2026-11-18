using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Department?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> GetAllForManagementAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsActiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments.AnyAsync(
            d => d.Id == id && d.IsActive,
            cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeDepartmentId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments.AnyAsync(
            d => d.Name == name &&
                 (!excludeDepartmentId.HasValue ||
                  d.Id != excludeDepartmentId.Value),
            cancellationToken);
    }

    public async Task<bool> HasActiveEmployeesAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees.AnyAsync(
            e => e.DepartmentId == departmentId && e.IsActive,
            cancellationToken);
    }

    public async Task AddAsync(
        Department department,
        CancellationToken cancellationToken = default)
    {
        await _context.Departments.AddAsync(department, cancellationToken);
    }

    public Task UpdateAsync(
        Department department,
        CancellationToken cancellationToken = default)
    {
        _context.Departments.Update(department);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
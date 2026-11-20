
using ITHelpdesk.Application.DTOs.TicketCategories;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Services;

public class TicketCategoryService : ITicketCategoryService
{
    private readonly ITicketCategoryRepository _categoryRepository;

    public TicketCategoryService(
        ITicketCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<TicketCategoryResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllForManagementAsync(
            cancellationToken);

        return categories.Select(MapToResponse).ToList();
    }

    public async Task<TicketCategoryResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(
            id, cancellationToken);

        if (category is null)
            throw new KeyNotFoundException("Ticket category not found.");

        return MapToResponse(category);
    }

    public async Task<Guid> CreateAsync(
        CreateTicketCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = NormalizeName(request.Name);

        if (await _categoryRepository.ExistsByNameAsync(
            name, cancellationToken: cancellationToken))
        {
            throw new ArgumentException(
                "A ticket category with this name already exists.");
        }

        var category = new TicketCategory
        {
            Name = name,
            Description = NormalizeDescription(request.Description),
            IsActive = true
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateTicketCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdForUpdateAsync(
            id, cancellationToken);

        if (category is null)
            throw new KeyNotFoundException("Ticket category not found.");

        var name = NormalizeName(request.Name);

        if (await _categoryRepository.ExistsByNameAsync(
            name, id, cancellationToken))
        {
            throw new ArgumentException(
                "A ticket category with this name already exists.");
        }

        category.Name = name;
        category.Description = NormalizeDescription(request.Description);
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdForUpdateAsync(
            id, cancellationToken);

        if (category is null)
            throw new KeyNotFoundException("Ticket category not found.");

        if (!category.IsActive)
            return;

        if (await _categoryRepository.HasTicketsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException(
                "Cannot deactivate a category that has tickets.");
        }

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.");

        return name.Trim();
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    private static TicketCategoryResponse MapToResponse(
        TicketCategory category)
    {
        return new TicketCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }
}


using ITHelpdesk.Application.DTOs.TicketCategories;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface ITicketCategoryService
{
    Task<IReadOnlyList<TicketCategoryResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<TicketCategoryResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        CreateTicketCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid id,
        UpdateTicketCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

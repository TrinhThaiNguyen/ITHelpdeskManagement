using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITicketCategoryRepository _categoryRepository;

    public TicketService(
        ITicketRepository ticketRepository,
        IDepartmentRepository departmentRepository,
        ITicketCategoryRepository categoryRepository)
    {
        _ticketRepository = ticketRepository;
        _departmentRepository = departmentRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> CreateTicketAsync(
        CreateTicketRequest request,
        Guid requesterId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Ticket title is required.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Ticket description is required.");

        if (requesterId == Guid.Empty)
            throw new ArgumentException("Requester ID is invalid.");

        if (!Enum.IsDefined(typeof(
            ITHelpdesk.Domain.Enums.TicketPriority), request.Priority))
        {
            throw new ArgumentException("Ticket priority is invalid.");
        }

        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId, cancellationToken);

        if (category is null || !category.IsActive)
            throw new InvalidOperationException(
                "Ticket category does not exist or is inactive.");

        var department = await _departmentRepository.GetByIdAsync(
            request.DepartmentId, cancellationToken);

        if (department is null || !department.IsActive)
            throw new InvalidOperationException(
                "Department does not exist or is inactive.");

        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            CategoryId = request.CategoryId,
            DepartmentId = request.DepartmentId,
            RequesterId = requesterId
        };

        await _ticketRepository.AddAsync(ticket, cancellationToken);

        return ticket.Id;
    }

    public async Task<TicketResponse?> GetTicketByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(
            id, cancellationToken);

        if (ticket is null)
            return null;

        return MapToResponse(ticket);
    }

    public async Task<IReadOnlyList<TicketResponse>> GetAllTicketsAsync(
        CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetAllAsync(
            cancellationToken);

        return tickets
            .Select(MapToResponse)
            .ToList();
    }

    private static TicketResponse MapToResponse(Ticket ticket)
    {
        return new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category.Name,
            DepartmentId = ticket.DepartmentId,
            DepartmentName = ticket.Department.Name,
            RequesterId = ticket.RequesterId,
            AssigneeId = ticket.AssigneeId,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };
    }
}
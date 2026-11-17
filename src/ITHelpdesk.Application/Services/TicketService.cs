using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITicketCategoryRepository _categoryRepository;
    private readonly ITicketHistoryRepository _ticketHistoryRepository;

    public TicketService(
        ITicketRepository ticketRepository,
        IDepartmentRepository departmentRepository,
        ITicketCategoryRepository categoryRepository,
        ITicketHistoryRepository ticketHistoryRepository)
    {
        _ticketRepository = ticketRepository;
        _departmentRepository = departmentRepository;
        _categoryRepository = categoryRepository;
        _ticketHistoryRepository = ticketHistoryRepository;

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

    public async Task<TicketResponse?> UpdateTicketAsync(
        Guid id,
        UpdateTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Tìm Ticket cần cập nhật
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(
            id,
            cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        // 2. Validate dữ liệu đầu vào
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Ticket title is required.");
        }

        if (request.Title.Length > 200)
        {
            throw new ArgumentException("Ticket title cannot exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException("Ticket description is required.");
        }

        if (request.Description.Length > 4000)
        {
            throw new ArgumentException("Ticket description cannot exceed 4000 characters.");
        }

        if (!Enum.IsDefined(typeof(TicketPriority), request.Priority))
        {
            throw new ArgumentException("Invalid ticket priority.");
        }

        // 3. Kiểm tra Category
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken);

        if (category is null)
        {
            throw new ArgumentException("Ticket category does not exist.");
        }

        // 4. Kiểm tra Department
        var department = await _departmentRepository.GetByIdAsync(
            request.DepartmentId,
            cancellationToken);

        if (department is null)
        {
            throw new ArgumentException("Department does not exist.");
        }

        // 5. Cập nhật thông tin Ticket
        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Priority = (TicketPriority)request.Priority;
        ticket.CategoryId = request.CategoryId;
        ticket.DepartmentId = request.DepartmentId;

        // 6. Lưu thay đổi
        await _ticketRepository.UpdateAsync(ticket, cancellationToken);

        // 7. Trả về dữ liệu sau cập nhật
        return MapToResponse(ticket);
    }

    public async Task<bool> DeleteTicketAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(
            id,
            cancellationToken);

        if (ticket is null)
        {
            return false;
        }

        await _ticketRepository.DeleteAsync(
            ticket,
            cancellationToken);

        return true;
    }


    public async Task<bool> UpdateTicketStatusAsync(
        Guid id,
        UpdateTicketStatusRequest request,
        string changedBy,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(
            id,
            cancellationToken);

        if (ticket is null)
            return false;

        if (!Enum.IsDefined(typeof(TicketStatus), request.Status))
            throw new ArgumentException("Invalid ticket status.");

        var newStatus = (TicketStatus)request.Status;

        if (!TicketStatusWorkflow.CanTransition(ticket.Status, newStatus))
        {
            throw new ArgumentException(
                $"Cannot change ticket status from {ticket.Status} to {newStatus}.");
        }

        var oldStatus = ticket.Status;

        // Update Ticket status in memory
        ticket.Status = newStatus;

        // Prepare history record
        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            Action = "StatusChanged",
            OldValue = oldStatus.ToString(),
            NewValue = newStatus.ToString(),
            ChangedBy = changedBy
        };

        // Add history to the shared DbContext (not saved yet)
        await _ticketHistoryRepository.AddAsync(
            history,
            cancellationToken);

        // Save Ticket and TicketHistory together
        await _ticketRepository.UpdateAsync(
            ticket,
            cancellationToken);

        return true;
    }

}
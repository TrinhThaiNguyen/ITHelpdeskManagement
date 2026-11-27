using ITHelpdesk.Application.DTOs.Pagination;
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
    private readonly ISlaPolicyRepository _slaPolicyRepository;
    private readonly ITicketSlaRepository _ticketSlaRepository;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientRepository _notificationRecipientRepository;


    public TicketService(
        ITicketRepository ticketRepository,
        IDepartmentRepository departmentRepository,
        ITicketCategoryRepository categoryRepository,
        ITicketHistoryRepository ticketHistoryRepository,
        ISlaPolicyRepository slaPolicyRepository,
        ITicketSlaRepository ticketSlaRepository,
        INotificationService notificationService,
        INotificationRecipientRepository notificationRecipientRepository)
    {
        _ticketRepository = ticketRepository;
        _departmentRepository = departmentRepository;
        _categoryRepository = categoryRepository;
        _ticketHistoryRepository = ticketHistoryRepository;
        _slaPolicyRepository = slaPolicyRepository;
        _ticketSlaRepository = ticketSlaRepository;
        _notificationService = notificationService;
        _notificationRecipientRepository = notificationRecipientRepository;

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

        var slaPolicy = await _slaPolicyRepository.GetActiveByPriorityAsync(
            request.Priority,
            cancellationToken);

        if (slaPolicy is null)
        {
            throw new InvalidOperationException(
                $"No active SLA policy found for priority {request.Priority}.");
        }

        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            CategoryId = request.CategoryId,
            DepartmentId = request.DepartmentId,
            RequesterId = requesterId
        };

        var trackedSlaPolicy = await _slaPolicyRepository.GetByIdAsync(
            slaPolicy.Id,
            cancellationToken);

        if (trackedSlaPolicy is null || !trackedSlaPolicy.IsActive)
        {
            throw new InvalidOperationException(
                "The selected SLA policy is no longer active.");
        }

        ticket.TicketSla = new TicketSla
        {
            TicketId = ticket.Id,
            SlaPolicyId = trackedSlaPolicy.Id,
            SlaPolicy = trackedSlaPolicy,

            ResponseDeadline = ticket.CreatedAt.AddMinutes(
                trackedSlaPolicy.ResponseTimeMinutes),

            ResolutionDeadline = ticket.CreatedAt.AddMinutes(
                trackedSlaPolicy.ResolutionTimeMinutes),

            ResponseBreached = false,
            ResolutionBreached = false
        };

        await _ticketRepository.AddAsync(ticket, cancellationToken);

        // Notify Admin and ITStaff
        await _notificationService.NotifyTicketCreatedAsync(
            ticket.Id,
            ticket.Title);

        return ticket.Id;
    }

    public async Task<TicketResponse?> GetTicketByIdAsync(
        Guid id,
        Guid? requesterId = null,
        CancellationToken cancellationToken = default)
    {
        Ticket? ticket;

        if (requesterId.HasValue)
        {
            ticket = await _ticketRepository.GetByIdAndRequesterIdAsync(
                id,
                requesterId.Value,
                cancellationToken);
        }
        else
        {
            ticket = await _ticketRepository.GetByIdAsync(
                id,
                cancellationToken);
        }

        if (ticket is null)
            return null;

        return MapToResponse(ticket);
    }

    public async Task<IReadOnlyList<TicketResponse>> GetAllTicketsAsync(
        Guid? requesterId = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Ticket> tickets;

        if (requesterId.HasValue)
        {
            tickets = await _ticketRepository.GetByRequesterIdAsync(
                requesterId.Value,
                cancellationToken);
        }
        else
        {
            tickets = await _ticketRepository.GetAllAsync(
                cancellationToken);
        }

        return tickets
            .Select(MapToResponse)
            .ToList();
    }


    public async Task<PagedResponse<TicketResponse>> GetPagedTicketsAsync(
        TicketQueryParameters parameters,
        Guid? requesterId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        // 1. Validate Pagination
        if (parameters.PageNumber < 1)
        {
            throw new ArgumentException(
                "Page number must be greater than or equal to 1.");
        }

        if (parameters.PageSize < 1 || parameters.PageSize > 100)
        {
            throw new ArgumentException(
                "Page size must be between 1 and 100.");
        }

        // 2. Validate SearchTerm
        if (parameters.SearchTerm?.Length > 200)
        {
            throw new ArgumentException(
                "Search term cannot exceed 200 characters.");
        }

        // 3. Validate Status
        if (parameters.Status.HasValue &&
            !Enum.IsDefined(typeof(TicketStatus), parameters.Status.Value))
        {
            throw new ArgumentException("Invalid ticket status.");
        }

        // 4. Validate Priority
        if (parameters.Priority.HasValue &&
            !Enum.IsDefined(typeof(TicketPriority), parameters.Priority.Value))
        {
            throw new ArgumentException("Invalid ticket priority.");
        }

        // 5. Query dữ liệu từ Repository
        var pagedTickets = await _ticketRepository.GetPagedAsync(
            parameters,
            requesterId,
            cancellationToken);

        // 6. Mapping Entity sang DTO
        return new PagedResponse<TicketResponse>
        {
            Items = pagedTickets.Items
                .Select(MapToResponse)
                .ToList(),

            PageNumber = pagedTickets.PageNumber,
            PageSize = pagedTickets.PageSize,
            TotalCount = pagedTickets.TotalCount
        };
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
            UpdatedAt = ticket.UpdatedAt,
            Sla = ticket.TicketSla == null
                ? null
                : new TicketSlaResponse
                {
                    SlaPolicyName = ticket.TicketSla.SlaPolicy.Name,
                    ResponseDeadline = ticket.TicketSla.ResponseDeadline,
                    ResolutionDeadline = ticket.TicketSla.ResolutionDeadline,
                    FirstResponseAt = ticket.TicketSla.FirstResponseAt,
                    ResolvedAt = ticket.TicketSla.ResolvedAt,
                    ResponseBreached = ticket.TicketSla.ResponseBreached,
                    ResolutionBreached = ticket.TicketSla.ResolutionBreached
                }
        };
    }

    public async Task<TicketResponse?> UpdateTicketAsync(
     Guid id,
     UpdateTicketRequest request,
     string changedBy,
     CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentException("Update request is required.");
        }

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new ArgumentException("ChangedBy is required.");
        }

        var ticket = await _ticketRepository.GetByIdForUpdateAsync(
            id,
            cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        // 1. Normalize and validate input
        var title = request.Title?.Trim();
        var description = request.Description?.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Ticket title is required.");
        }

        if (title.Length > 200)
        {
            throw new ArgumentException("Ticket title cannot exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Ticket description is required.");
        }

        if (description.Length > 4000)
        {
            throw new ArgumentException("Ticket description cannot exceed 4000 characters.");
        }

        if (!Enum.IsDefined(typeof(TicketPriority), request.Priority))
        {
            throw new ArgumentException("Invalid ticket priority.");
        }

        var newPriority = (TicketPriority)request.Priority;

        // 2. Validate Category
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken);

        if (category is null || !category.IsActive)
        {
            throw new ArgumentException(
                "Ticket category does not exist or is inactive.");
        }

        // 3. Validate Department
        var department = await _departmentRepository.GetByIdAsync(
            request.DepartmentId,
            cancellationToken);

        if (department is null || !department.IsActive)
        {
            throw new ArgumentException(
                "Department does not exist or is inactive.");
        }

        // 4. Record changes before updating the Ticket
        if (ticket.Title != title)
        {
            await _ticketHistoryRepository.AddAsync(
                new TicketHistory
                {
                    TicketId = ticket.Id,
                    Action = "TitleChanged",
                    OldValue = ticket.Title,
                    NewValue = title,
                    ChangedBy = changedBy
                },
                cancellationToken);

            ticket.Title = title;
        }

        if (ticket.Description != description)
        {
            await _ticketHistoryRepository.AddAsync(
                new TicketHistory
                {
                    TicketId = ticket.Id,
                    Action = "DescriptionChanged",
                    OldValue = ticket.Description,
                    NewValue = description,
                    ChangedBy = changedBy
                },
                cancellationToken);

            ticket.Description = description;
        }

        if (ticket.Priority != newPriority)
        {
            var oldPriority = ticket.Priority;

            var slaPolicy = await _slaPolicyRepository.GetActiveByPriorityAsync(
                newPriority,
                cancellationToken);

            if (slaPolicy is null)
            {
                throw new ArgumentException(
                    "No active SLA policy exists for the selected priority.");
            }

            if (ticket.TicketSla is null)
            {
                throw new InvalidOperationException(
                    "Ticket SLA information is missing.");
            }

            var now = DateTime.UtcNow;

            var trackedSlaPolicy = await _slaPolicyRepository.GetByIdAsync(
                slaPolicy.Id,
                cancellationToken);

            if (trackedSlaPolicy is null || !trackedSlaPolicy.IsActive)
            {
                throw new InvalidOperationException(
                    "The selected SLA policy is no longer active.");
            }

            ticket.TicketSla.SlaPolicyId = trackedSlaPolicy.Id;
            ticket.TicketSla.SlaPolicy = trackedSlaPolicy;

            // Only update SLA deadlines that have not been completed.
            if (ticket.TicketSla.FirstResponseAt is null)
            {
                ticket.TicketSla.ResponseDeadline =
                    now.AddMinutes(slaPolicy.ResponseTimeMinutes);

                ticket.TicketSla.ResponseBreached = false;
            }

            if (ticket.TicketSla.ResolvedAt is null)
            {
                ticket.TicketSla.ResolutionDeadline =
                    now.AddMinutes(slaPolicy.ResolutionTimeMinutes);

                ticket.TicketSla.ResolutionBreached = false;
            }

            await _ticketHistoryRepository.AddAsync(
                new TicketHistory
                {
                    TicketId = ticket.Id,
                    Action = "PriorityChanged",
                    OldValue = oldPriority.ToString(),
                    NewValue = newPriority.ToString(),
                    ChangedBy = changedBy
                },
                cancellationToken);

            ticket.Priority = newPriority;
        }

        if (ticket.CategoryId != request.CategoryId)
        {
            await _ticketHistoryRepository.AddAsync(
                new TicketHistory
                {
                    TicketId = ticket.Id,
                    Action = "CategoryChanged",
                    OldValue = ticket.CategoryId.ToString(),
                    NewValue = request.CategoryId.ToString(),
                    ChangedBy = changedBy
                },
                cancellationToken);

            ticket.CategoryId = request.CategoryId;
            ticket.Category = category;
        }

        if (ticket.DepartmentId != request.DepartmentId)
        {
            await _ticketHistoryRepository.AddAsync(
                new TicketHistory
                {
                    TicketId = ticket.Id,
                    Action = "DepartmentChanged",
                    OldValue = ticket.DepartmentId.ToString(),
                    NewValue = request.DepartmentId.ToString(),
                    ChangedBy = changedBy
                },
                cancellationToken);

            ticket.DepartmentId = request.DepartmentId;
            ticket.Department = department;
        }

        // 5. Save Ticket, SLA and history changes together
        await _ticketRepository.UpdateAsync(
            ticket,
            cancellationToken);

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
    Guid changedByUserId,
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

        ticket.Status = newStatus;

        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            Action = "StatusChanged",
            OldValue = oldStatus.ToString(),
            NewValue = newStatus.ToString(),
            ChangedBy = changedBy
        };

        await _ticketHistoryRepository.AddAsync(
            history,
            cancellationToken);

        await _ticketRepository.UpdateAsync(
            ticket,
            cancellationToken);

        if (newStatus == TicketStatus.Resolved)
        {
            await _ticketSlaRepository.RecordResolutionAsync(
                ticket.Id,
                DateTime.UtcNow,
                cancellationToken);
        }

        // Find notification recipients.
        var recipientIds = new HashSet<Guid>();

        var requesterUserId =
            await _notificationRecipientRepository.GetUserIdByEmployeeIdAsync(
                ticket.RequesterId);

        if (requesterUserId.HasValue)
            recipientIds.Add(requesterUserId.Value);

        if (ticket.AssigneeId.HasValue)
        {
            var assigneeUserId =
                await _notificationRecipientRepository.GetUserIdByEmployeeIdAsync(
                    ticket.AssigneeId.Value);

            if (assigneeUserId.HasValue)
                recipientIds.Add(assigneeUserId.Value);
        }

        // Exclude the user who changed the status.
        recipientIds.Remove(changedByUserId);

        foreach (var recipientId in recipientIds)
        {
            await _notificationService.CreateAsync(
                recipientId,
                "Ticket Status Changed",
                $"Ticket '{ticket.Title}' status changed from {oldStatus} to {newStatus}.",
                NotificationType.TicketStatusChanged,
                ticket.Id);
        }

        return true;
    }

    public async Task<IReadOnlyList<TicketHistoryResponse>> GetTicketHistoryAsync(
        Guid ticketId,
        Guid? requesterId = null,
        CancellationToken cancellationToken = default)
    {
        Ticket? ticket;

        if (requesterId.HasValue)
        {
            ticket = await _ticketRepository.GetByIdAndRequesterIdAsync(
                ticketId,
                requesterId.Value,
                cancellationToken);
        }
        else
        {
            ticket = await _ticketRepository.GetByIdAsync(
                ticketId,
                cancellationToken);
        }

        if (ticket is null)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }

        var histories = await _ticketHistoryRepository.GetByTicketIdAsync(
            ticketId,
            cancellationToken);

        return histories.Select(history => new TicketHistoryResponse
        {
            Id = history.Id,
            Action = history.Action,
            OldValue = history.OldValue,
            NewValue = history.NewValue,
            ChangedBy = history.ChangedBy,
            CreatedAt = history.CreatedAt
        }).ToList();
    }

}
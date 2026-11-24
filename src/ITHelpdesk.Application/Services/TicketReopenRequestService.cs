using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.Services;

public class TicketReopenRequestService : ITicketReopenRequestService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketReopenRequestRepository _reopenRequestRepository;
    private readonly ITicketHistoryRepository _ticketHistoryRepository;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientRepository _recipientRepository;

    public TicketReopenRequestService(
        ITicketRepository ticketRepository,
        ITicketReopenRequestRepository reopenRequestRepository,
        ITicketHistoryRepository ticketHistoryRepository,
        INotificationService notificationService,
        INotificationRecipientRepository recipientRepository)
    {
        _ticketRepository = ticketRepository;
        _reopenRequestRepository = reopenRequestRepository;
        _ticketHistoryRepository = ticketHistoryRepository;
        _notificationService = notificationService;
        _recipientRepository = recipientRepository;
    }

    public async Task<TicketReopenRequestResponse> CreateAsync(
    Guid ticketId,
    Guid requesterUserId,
    Guid requesterEmployeeId,
    CreateTicketReopenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (requesterUserId == Guid.Empty ||
            requesterEmployeeId == Guid.Empty)
        {
            throw new ArgumentException("Requester identity is invalid.");
        }

        var reason = request.Reason?.Trim();

        if (string.IsNullOrWhiteSpace(reason) ||
            reason.Length < 10 ||
            reason.Length > 1000)
        {
            throw new ArgumentException(
                "Reason must contain between 10 and 1000 characters.");
        }

        // 1. Get tracked Ticket with SLA information.
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(ticketId);

        if (ticket is null || ticket.RequesterId != requesterEmployeeId)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }

        // 2. Only Resolved or Closed tickets can be reopened.
        if (ticket.Status != TicketStatus.Resolved &&
            ticket.Status != TicketStatus.Closed)
        {
            throw new InvalidOperationException(
                "Only resolved or closed tickets can be reopened.");
        }

        // 3. Prevent duplicate pending requests.
        if (await _reopenRequestRepository.HasPendingRequestAsync(ticketId))
        {
            throw new InvalidOperationException(
                "A pending reopen request already exists.");
        }

        // 4. Create reopen request.
        var reopenRequest = new TicketReopenRequest
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            RequestedByUserId = requesterUserId,
            Reason = reason,
            Status = TicketReopenRequestStatus.Pending
        };

        await _reopenRequestRepository.AddAsync(reopenRequest);

        // 5. Record ticket history.
        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            Action = "TicketReopenRequested",
            OldValue = ticket.Status.ToString(),
            NewValue = $"Pending: {reason}",
            ChangedBy = requesterUserId.ToString()
        };

        await _ticketHistoryRepository.AddAsync(history);

        // 6. Save request and history together.
        await _reopenRequestRepository.SaveChangesAsync();

        // 7. Notify Admin and ITStaff.
        var recipientIds = await _recipientRepository.GetUserIdsByRolesAsync(
            new[] { "Admin", "ITStaff" });

        recipientIds.Remove(requesterUserId);

        foreach (var recipientId in recipientIds)
        {
            await _notificationService.CreateAsync(
                recipientId,
                "Ticket Reopen Requested",
                $"Requester has requested to reopen ticket '{ticket.Title}'.",
                NotificationType.TicketReopenRequested,
                ticket.Id);
        }

        return MapToResponse(reopenRequest);
    }

    private static TicketReopenRequestResponse MapToResponse(
    TicketReopenRequest request)
    {
        return new TicketReopenRequestResponse
        {
            Id = request.Id,
            TicketId = request.TicketId,
            RequestedByUserId = request.RequestedByUserId,
            Reason = request.Reason,
            Status = request.Status,
            ReviewedByUserId = request.ReviewedByUserId,
            ReviewReason = request.ReviewReason,
            CreatedAt = request.CreatedAt,
            ReviewedAt = request.ReviewedAt
        };
    }

    public async Task<TicketReopenRequestResponse> ReviewAsync(
    Guid requestId,
    Guid reviewerUserId,
    ReviewTicketReopenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (reviewerUserId == Guid.Empty)
            throw new ArgumentException("Reviewer ID is invalid.");

        // 1. Validate review reason.
        var reviewReason = request.ReviewReason?.Trim();

        if (!request.IsApproved && string.IsNullOrWhiteSpace(reviewReason))
        {
            throw new ArgumentException(
                "Review reason is required when rejecting a request.");
        }

        if (reviewReason?.Length > 1000)
            throw new ArgumentException("Review reason cannot exceed 1000 characters.");

        // 2. Get reopen request.
        var reopenRequest = await _reopenRequestRepository.GetByIdAsync(requestId);

        if (reopenRequest is null)
            throw new KeyNotFoundException("Reopen request not found.");

        if (reopenRequest.Status != TicketReopenRequestStatus.Pending)
        {
            throw new InvalidOperationException(
                "This reopen request has already been reviewed.");
        }

        // 3. Get tracked Ticket with SLA.
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(
            reopenRequest.TicketId);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        if (ticket.Status != TicketStatus.Resolved &&
            ticket.Status != TicketStatus.Closed)
        {
            throw new InvalidOperationException(
                "Ticket is no longer in a status that can be reopened.");
        }

        var now = DateTime.UtcNow;
        var oldStatus = ticket.Status;

        // 4. Update review information.
        reopenRequest.ReviewedByUserId = reviewerUserId;
        reopenRequest.ReviewedAt = now;
        reopenRequest.ReviewReason = reviewReason;

        if (request.IsApproved)
        {
            reopenRequest.Status = TicketReopenRequestStatus.Approved;

            ticket.Status = TicketStatus.InProgress;

            // Restart resolution SLA.
            if (ticket.TicketSla is not null)
            {
                ticket.TicketSla.ResolutionDeadline =
                    now.AddMinutes(
                        ticket.TicketSla.SlaPolicy.ResolutionTimeMinutes);

                ticket.TicketSla.ResolvedAt = null;
                ticket.TicketSla.ResolutionBreached = false;
            }
        }
        else
        {
            reopenRequest.Status = TicketReopenRequestStatus.Rejected;
        }

        // 5. Record ticket history.
        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            Action = request.IsApproved
                ? "TicketReopenApproved"
                : "TicketReopenRejected",
            OldValue = oldStatus.ToString(),
            NewValue = request.IsApproved
                ? TicketStatus.InProgress.ToString()
                : $"Rejected: {reviewReason}",
            ChangedBy = reviewerUserId.ToString()
        };

        await _ticketHistoryRepository.AddAsync(history);

        // 6. Save request, history, Ticket and SLA together.
        await _ticketRepository.UpdateAsync(ticket);

        // 7. Notify requester.
        var title = request.IsApproved
            ? "Ticket Reopen Approved"
            : "Ticket Reopen Rejected";

        var message = request.IsApproved
            ? $"Your request to reopen ticket '{ticket.Title}' has been approved."
            : $"Your request to reopen ticket '{ticket.Title}' has been rejected. Reason: {reviewReason}";

        await _notificationService.CreateAsync(
            reopenRequest.RequestedByUserId,
            title,
            message,
            request.IsApproved
                ? NotificationType.TicketReopenApproved
                : NotificationType.TicketReopenRejected,
            ticket.Id);

        return MapToResponse(reopenRequest);
    }

    public async Task<List<TicketReopenRequestResponse>> GetByTicketIdAsync(
        Guid ticketId,
        Guid? requesterEmployeeId = null)
    {
        // Requesters can only view reopen requests for their own tickets.
        var ticket = requesterEmployeeId.HasValue
            ? await _ticketRepository.GetByIdAndRequesterIdAsync(
                ticketId,
                requesterEmployeeId.Value)
            : await _ticketRepository.GetByIdAsync(ticketId);

        if (ticket is null)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }

        var requests = await _reopenRequestRepository.GetByTicketIdAsync(
            ticketId);

        return requests
            .Select(MapToResponse)
            .ToList();
    }
}
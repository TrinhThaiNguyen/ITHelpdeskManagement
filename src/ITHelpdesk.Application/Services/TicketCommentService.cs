
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Services;

public class TicketCommentService : ITicketCommentService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketCommentRepository _commentRepository;
    private readonly ITicketSlaRepository _ticketSlaRepository;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientRepository _notificationRecipientRepository;

    public TicketCommentService(
        ITicketRepository ticketRepository,
        ITicketCommentRepository commentRepository,
        ITicketSlaRepository ticketSlaRepository,
        INotificationService notificationService,
        INotificationRecipientRepository notificationRecipientRepository)
    {
        _ticketRepository = ticketRepository;
        _commentRepository = commentRepository;
        _ticketSlaRepository = ticketSlaRepository;
        _notificationService = notificationService;
        _notificationRecipientRepository = notificationRecipientRepository;
    }

    public async Task<Guid> AddCommentAsync(
        Guid ticketId,
        CreateTicketCommentRequest request,
        Guid createdByUserId,
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
            throw new KeyNotFoundException("Ticket not found.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Comment content is required.");

        if (request.Content.Length > 4000)
            throw new ArgumentException(
                "Comment content cannot exceed 4000 characters.");

        var comment = new TicketComment
        {
            TicketId = ticketId,
            CreatedByUserId = createdByUserId,
            Content = request.Content.Trim(),
            IsInternal = requesterId.HasValue ? false : request.IsInternal
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        // Only public comments from IT Staff/Admin count as first response.
        if (!requesterId.HasValue && !comment.IsInternal)
        {
            await _ticketSlaRepository.RecordFirstResponseAsync(
                ticketId,
                comment.CreatedAt,
                cancellationToken);
        }

        // Determine notification recipients.
        var recipientIds = new HashSet<Guid>();

        if (comment.IsInternal)
        {
            // Internal comments are visible only to Admin and ITStaff.
            var staffIds = await _notificationRecipientRepository
                .GetUserIdsByRolesAsync(new[] { "Admin", "ITStaff" });

            foreach (var userId in staffIds)
                recipientIds.Add(userId);
        }
        else
        {
            // Public comments notify the requester.
            var requesterUserId = await _notificationRecipientRepository
                .GetUserIdByEmployeeIdAsync(ticket.RequesterId);

            if (requesterUserId.HasValue)
                recipientIds.Add(requesterUserId.Value);

            // Also notify the assigned employee, if any.
            if (ticket.AssigneeId.HasValue)
            {
                var assigneeUserId = await _notificationRecipientRepository
                    .GetUserIdByEmployeeIdAsync(ticket.AssigneeId.Value);

                if (assigneeUserId.HasValue)
                    recipientIds.Add(assigneeUserId.Value);
            }

            // If there is no assignee, notify Admin and ITStaff.
            else
            {
                var staffIds = await _notificationRecipientRepository
                    .GetUserIdsByRolesAsync(new[] { "Admin", "ITStaff" });

                foreach (var userId in staffIds)
                    recipientIds.Add(userId);
            }
        }

        // Never notify the comment author.
        recipientIds.Remove(createdByUserId);

        foreach (var userId in recipientIds)
        {
            await _notificationService.CreateAsync(
                userId,
                comment.IsInternal
                    ? "New Internal Comment"
                    : "New Comment",
                $"A new comment was added to ticket: {ticket.Title}",
                ITHelpdesk.Domain.Enums.NotificationType.NewComment,
                ticketId);
        }

        return comment.Id;
    }

    public async Task<IReadOnlyList<TicketCommentResponse>> GetCommentsAsync(
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
            throw new KeyNotFoundException("Ticket not found.");

        var comments = await _commentRepository.GetByTicketIdAsync(
            ticketId,
            cancellationToken);

        // Requester không ???c xem Internal Comment.
        return comments
            .Where(comment => !requesterId.HasValue || !comment.IsInternal)
            .Select(comment => new TicketCommentResponse
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                Content = comment.Content,
                IsInternal = comment.IsInternal,
                CreatedAt = comment.CreatedAt
            })
            .ToList();
    }
}

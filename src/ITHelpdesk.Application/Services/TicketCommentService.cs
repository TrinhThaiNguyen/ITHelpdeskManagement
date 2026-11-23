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

    public TicketCommentService(
        ITicketRepository ticketRepository,
        ITicketCommentRepository commentRepository,
        ITicketSlaRepository ticketSlaRepository)
    {
        _ticketRepository = ticketRepository;
        _commentRepository = commentRepository;
        _ticketSlaRepository = ticketSlaRepository;

    }

    public async Task<Guid> AddCommentAsync(
        Guid ticketId,
        CreateTicketCommentRequest request,
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
            Content = request.Content.Trim(),

            // Requester không ???c t?o Internal Comment
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

        // Requester không ???c xem Internal Comment
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
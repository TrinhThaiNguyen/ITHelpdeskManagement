
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Services;

public class TicketCommentService : ITicketCommentService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketCommentRepository _commentRepository;

    public TicketCommentService(
        ITicketRepository ticketRepository,
        ITicketCommentRepository commentRepository)
    {
        _ticketRepository = ticketRepository;
        _commentRepository = commentRepository;
    }

    public async Task<Guid> AddCommentAsync(
        Guid ticketId,
        CreateTicketCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(
            ticketId,
            cancellationToken);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Comment content is required.");

        if (request.Content.Length > 4000)
            throw new ArgumentException("Comment content cannot exceed 4000 characters.");

        var comment = new TicketComment
        {
            TicketId = ticketId,
            Content = request.Content.Trim(),
            IsInternal = request.IsInternal
        };

        await _commentRepository.AddAsync(comment, cancellationToken);

        await _commentRepository.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }

    public async Task<IReadOnlyList<TicketCommentResponse>> GetCommentsAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(
            ticketId,
            cancellationToken);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");

        var comments = await _commentRepository.GetByTicketIdAsync(
            ticketId,
            cancellationToken);

        return comments.Select(comment => new TicketCommentResponse
        {
            Id = comment.Id,
            TicketId = comment.TicketId,
            Content = comment.Content,
            IsInternal = comment.IsInternal,
            CreatedAt = comment.CreatedAt
        }).ToList();
    }
}

using ITHelpdesk.Application.DTOs.Tickets;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface ITicketAttachmentService
{
    Task<Guid> UploadAsync(
        Guid ticketId,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketAttachmentResponse>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<Stream?> DownloadAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default);
}
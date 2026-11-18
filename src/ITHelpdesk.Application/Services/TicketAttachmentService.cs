using ITHelpdesk.Application.Common.Interfaces;
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Services;

public class TicketAttachmentService : ITicketAttachmentService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketAttachmentRepository _attachmentRepository;
    private readonly IFileStorageService _fileStorageService;

    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
    [
        ".pdf", ".png", ".jpg", ".jpeg",
        ".txt", ".docx", ".xlsx"
    ];

    public TicketAttachmentService(
        ITicketRepository ticketRepository,
        ITicketAttachmentRepository attachmentRepository,
        IFileStorageService fileStorageService)
    {
        _ticketRepository = ticketRepository;
        _attachmentRepository = attachmentRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Guid> UploadAsync(
        Guid ticketId,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(
            ticketId, cancellationToken);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found.");

        if (fileStream == null || !fileStream.CanRead)
            throw new ArgumentException("Invalid file stream.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.");

        if (fileSizeBytes <= 0 || fileSizeBytes > MaxFileSize)
            throw new ArgumentException("File size must be between 1 byte and 10 MB.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("File extension is not allowed.");

        var safeFileName = Path.GetFileName(fileName);

        if (safeFileName.Length > 255)
            throw new ArgumentException("File name cannot exceed 255 characters.");

        string storageKey = await _fileStorageService.SaveFileAsync(
            fileStream,
            safeFileName,
            cancellationToken);

        try
        {
            var attachment = new TicketAttachment
            {
                TicketId = ticketId,
                FileName = safeFileName,
                StorageKey = storageKey,
                ContentType = string.IsNullOrWhiteSpace(contentType)
                    ? "application/octet-stream"
                    : contentType,
                FileSizeBytes = fileSizeBytes
            };

            await _attachmentRepository.AddAsync(
                attachment, cancellationToken);

            await _attachmentRepository.SaveChangesAsync(cancellationToken);

            return attachment.Id;
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(
                storageKey, cancellationToken);

            throw;
        }
    }

    public async Task<IReadOnlyList<TicketAttachmentResponse>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(
            ticketId, cancellationToken);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found.");

        var attachments = await _attachmentRepository.GetByTicketIdAsync(
            ticketId, cancellationToken);

        return attachments.Select(a => new TicketAttachmentResponse
        {
            Id = a.Id,
            TicketId = a.TicketId,
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSizeBytes = a.FileSizeBytes,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task<Stream?> DownloadAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(
            attachmentId, cancellationToken);

        if (attachment == null)
            throw new KeyNotFoundException("Attachment not found.");

        return await _fileStorageService.GetFileAsync(
            attachment.StorageKey, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(
            attachmentId, cancellationToken);

        if (attachment == null)
            throw new KeyNotFoundException("Attachment not found.");

        await _attachmentRepository.DeleteAsync(
            attachment, cancellationToken);

        await _attachmentRepository.SaveChangesAsync(cancellationToken);

        await _fileStorageService.DeleteFileAsync(
            attachment.StorageKey, cancellationToken);
    }
}
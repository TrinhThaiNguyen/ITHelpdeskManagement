namespace ITHelpdesk.Application.DTOs.Tickets;

public class TicketAttachmentResponse
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public DateTime CreatedAt { get; set; }
}
namespace ITHelpdesk.Application.DTOs.Tickets;

public class UploadTicketAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }
}
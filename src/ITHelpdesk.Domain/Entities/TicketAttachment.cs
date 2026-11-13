using ITHelpdesk.Domain.Common;

namespace ITHelpdesk.Domain.Entities;

public class TicketAttachment : BaseEntity
{
    public Guid TicketId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public Ticket Ticket { get; set; } = null!;
}

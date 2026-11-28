
namespace ITHelpdeshMangament.Models.Tickets;

public class TicketCommentViewModel
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsInternal { get; set; }

    public DateTime CreatedAt { get; set; }
}

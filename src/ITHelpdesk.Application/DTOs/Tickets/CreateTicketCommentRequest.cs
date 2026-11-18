
namespace ITHelpdesk.Application.DTOs.Tickets;

public class CreateTicketCommentRequest
{
    public string Content { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;
}

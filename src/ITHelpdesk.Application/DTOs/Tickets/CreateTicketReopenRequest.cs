
using System.ComponentModel.DataAnnotations;

namespace ITHelpdesk.Application.DTOs.Tickets;

public class CreateTicketReopenRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}

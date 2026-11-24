
using System.ComponentModel.DataAnnotations;

namespace ITHelpdesk.Application.DTOs.Tickets;

public class ReviewTicketReopenRequest
{
    [Required]
    public bool IsApproved { get; set; }

    [StringLength(1000)]
    public string? ReviewReason { get; set; }
}

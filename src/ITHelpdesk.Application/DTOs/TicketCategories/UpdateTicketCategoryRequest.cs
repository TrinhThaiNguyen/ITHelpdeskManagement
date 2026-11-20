using System.ComponentModel.DataAnnotations;

namespace ITHelpdesk.Application.DTOs.TicketCategories;

public class UpdateTicketCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
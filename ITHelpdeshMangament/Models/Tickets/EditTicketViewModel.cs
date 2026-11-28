
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ITHelpdeshMangament.Models.Tickets;

public class EditTicketViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 4)]
    public int Priority { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }

    public List<SelectListItem> Categories { get; set; } = [];

    public List<SelectListItem> Departments { get; set; } = [];
}

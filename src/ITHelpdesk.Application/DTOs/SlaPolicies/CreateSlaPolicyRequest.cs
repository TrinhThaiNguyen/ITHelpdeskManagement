using ITHelpdesk.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ITHelpdesk.Application.DTOs.SlaPolicies;

public class CreateSlaPolicyRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority Priority { get; set; }

    [Range(1, 525600)]
    public int ResponseTimeMinutes { get; set; }

    [Range(1, 525600)]
    public int ResolutionTimeMinutes { get; set; }
}
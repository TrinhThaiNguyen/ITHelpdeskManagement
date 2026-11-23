using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.DTOs.SlaPolicies;

public class SlaPolicyResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; }

    public int ResponseTimeMinutes { get; set; }

    public int ResolutionTimeMinutes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.DTOs.Tickets;

public class TicketResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; }

    public TicketPriority Priority { get; set; }

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public Guid RequesterId { get; set; }

    public Guid? AssigneeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
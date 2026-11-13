using ITHelpdesk.Domain.Common;

namespace ITHelpdesk.Domain.Entities;

public class Employee : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public Guid DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<Ticket> RequestedTickets { get; set; }
        = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; }
        = new List<Ticket>();
}

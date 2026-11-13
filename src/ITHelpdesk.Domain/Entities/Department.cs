using ITHelpdesk.Domain.Common;

namespace ITHelpdesk.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Ticket> Tickets { get; set; }
        = new List<Ticket>();

    public ICollection<Employee> Employees { get; set; }
    = new List<Employee>();
}

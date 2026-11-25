
namespace ITHelpdesk.Application.DTOs.Dashboard;

public class StaffWorkloadResponse
{
    public Guid EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int TotalAssignedTickets { get; set; }

    public int NewTickets { get; set; }

    public int OpenTickets { get; set; }

    public int InProgressTickets { get; set; }

    public int PendingTickets { get; set; }
}

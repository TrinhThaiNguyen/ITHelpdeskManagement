
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.DTOs.Dashboard;

public class TicketStatusDistributionResponse
{
    public TicketStatus Status { get; set; }

    public int TicketCount { get; set; }

    public double Percentage { get; set; }
}

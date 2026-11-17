using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.Services;

public static class TicketStatusWorkflow
{
    public static bool CanTransition(
        TicketStatus currentStatus,
        TicketStatus newStatus)
    {
        return currentStatus switch
        {
            TicketStatus.New =>
                newStatus == TicketStatus.Open,

            TicketStatus.Open =>
                newStatus == TicketStatus.InProgress,

            TicketStatus.InProgress =>
                newStatus == TicketStatus.Pending ||
                newStatus == TicketStatus.Resolved,

            TicketStatus.Pending =>
                newStatus == TicketStatus.InProgress ||
                newStatus == TicketStatus.Resolved,

            TicketStatus.Resolved =>
                newStatus == TicketStatus.Closed,

            TicketStatus.Closed => false,

            _ => false
        };
    }
}
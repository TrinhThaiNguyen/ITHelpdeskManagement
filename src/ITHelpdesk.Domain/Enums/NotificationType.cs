namespace ITHelpdesk.Domain.Enums;

public enum NotificationType
{
    TicketCreated = 1,
    TicketAssigned = 2,
    NewComment = 3,
    TicketStatusChanged = 4,
    TicketReopenRequested = 5,
    TicketReopenApproved = 6,
    TicketReopenRejected = 7
}
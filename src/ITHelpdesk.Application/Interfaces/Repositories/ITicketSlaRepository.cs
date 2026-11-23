using ITHelpdesk.Application.DTOs.Tickets;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketSlaRepository
{
    Task RecordFirstResponseAsync(
        Guid ticketId,
        DateTime responseAt,
        CancellationToken cancellationToken = default);

    Task RecordResolutionAsync(
        Guid ticketId,
        DateTime resolvedAt,
        CancellationToken cancellationToken = default);

    Task<SlaMonitoringResponse> GetMonitoringAsync(
        DateTime currentTime,
        CancellationToken cancellationToken = default);
}
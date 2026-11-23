using ITHelpdesk.Application.DTOs.Tickets;

namespace ITHelpdesk.Application.Interfaces.Services;

public interface ISlaMonitoringService
{
    Task<SlaMonitoringResponse> GetMonitoringAsync(
        CancellationToken cancellationToken = default);
}
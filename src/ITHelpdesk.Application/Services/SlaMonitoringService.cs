using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;

namespace ITHelpdesk.Application.Services;

public class SlaMonitoringService : ISlaMonitoringService
{
    private readonly ITicketSlaRepository _ticketSlaRepository;

    public SlaMonitoringService(
        ITicketSlaRepository ticketSlaRepository)
    {
        _ticketSlaRepository = ticketSlaRepository;
    }

    public Task<SlaMonitoringResponse> GetMonitoringAsync(
        CancellationToken cancellationToken = default)
    {
        return _ticketSlaRepository.GetMonitoringAsync(
            DateTime.UtcNow,
            cancellationToken);
    }
}
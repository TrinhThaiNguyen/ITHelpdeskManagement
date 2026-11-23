using ITHelpdesk.Domain.Entities;
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ISlaPolicyRepository
{
    Task<SlaPolicy?> GetActiveByPriorityAsync(
        TicketPriority priority,
        CancellationToken cancellationToken = default);
}
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketHistoryRepository
{
    Task AddAsync(
        TicketHistory history,
        CancellationToken cancellationToken = default);
}
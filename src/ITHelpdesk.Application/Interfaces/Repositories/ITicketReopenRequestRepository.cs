
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Interfaces.Repositories;

public interface ITicketReopenRequestRepository
{
    Task<TicketReopenRequest?> GetByIdAsync(Guid id);

    Task<bool> HasPendingRequestAsync(Guid ticketId);

    Task<List<TicketReopenRequest>> GetByTicketIdAsync(Guid ticketId);

    Task AddAsync(TicketReopenRequest request);

    Task SaveChangesAsync();
}


using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Enums;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class TicketSlaRepository : ITicketSlaRepository
{
    private readonly AppDbContext _context;

    public TicketSlaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task RecordFirstResponseAsync(
        Guid ticketId,
        DateTime responseAt,
        CancellationToken cancellationToken = default)
    {
        await _context.TicketSlas
            .Where(x =>
                x.TicketId == ticketId &&
                x.FirstResponseAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.FirstResponseAt,
                        (DateTime?)responseAt)
                    .SetProperty(
                        x => x.ResponseBreached,
                        x => responseAt > x.ResponseDeadline),
                cancellationToken);
    }

    public async Task RecordResolutionAsync(
    Guid ticketId,
    DateTime resolvedAt,
    CancellationToken cancellationToken = default)
    {
        await _context.TicketSlas
            .Where(x =>
                x.TicketId == ticketId &&
                x.ResolvedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.ResolvedAt,
                        (DateTime?)resolvedAt)
                    .SetProperty(
                        x => x.ResolutionBreached,
                        x => resolvedAt > x.ResolutionDeadline),
                cancellationToken);
    }

    public async Task<SlaMonitoringResponse> GetMonitoringAsync(
    DateTime currentTime,
    CancellationToken cancellationToken = default)
    {
        var upcomingDeadline = currentTime.AddMinutes(60);

        var query = _context.TicketSlas
            .AsNoTracking()
            .Where(ts => !ts.Ticket.IsDeleted);

        return new SlaMonitoringResponse
        {
            TotalTicketsWithSla = await query.CountAsync(
                cancellationToken),

            ResponseOverdueCount = await query.CountAsync(
                ts => ts.FirstResponseAt == null
                      && ts.ResponseDeadline < currentTime
                      && ts.Ticket.Status != TicketStatus.Resolved
                      && ts.Ticket.Status != TicketStatus.Closed,
                cancellationToken),

            ResolutionOverdueCount = await query.CountAsync(
                ts => ts.ResolvedAt == null
                      && ts.ResolutionDeadline < currentTime
                      && ts.Ticket.Status != TicketStatus.Resolved
                      && ts.Ticket.Status != TicketStatus.Closed,
                cancellationToken),

            ResponseBreachedCount = await query.CountAsync(
                ts => ts.ResponseBreached,
                cancellationToken),

            ResolutionBreachedCount = await query.CountAsync(
                ts => ts.ResolutionBreached,
                cancellationToken),

            UpcomingResponseDeadlineCount = await query.CountAsync(
                ts => ts.FirstResponseAt == null
                      && ts.ResponseDeadline >= currentTime
                      && ts.ResponseDeadline <= upcomingDeadline
                      && ts.Ticket.Status != TicketStatus.Resolved
                      && ts.Ticket.Status != TicketStatus.Closed,
                cancellationToken),

            UpcomingResolutionDeadlineCount = await query.CountAsync(
                ts => ts.ResolvedAt == null
                      && ts.ResolutionDeadline >= currentTime
                      && ts.ResolutionDeadline <= upcomingDeadline
                      && ts.Ticket.Status != TicketStatus.Resolved
                      && ts.Ticket.Status != TicketStatus.Closed,
                cancellationToken)
        };
    }
}

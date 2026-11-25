
using ITHelpdesk.Application.DTOs.Dashboard;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Enums;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class DashboardExportRepository : IDashboardExportRepository
{
    private readonly AppDbContext _context;

    public DashboardExportRepository(AppDbContext context)
    {
        _context = context;
    }

    private static (DateTime StartDate, DateTime EndDate) GetDateRange(
        DateOnly fromDate,
        DateOnly toDate)
    {
        return (
            fromDate.ToDateTime(TimeOnly.MinValue),
            toDate.AddDays(1).ToDateTime(TimeOnly.MinValue)
        );
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) = GetDateRange(fromDate, toDate);

        var result = await _context.Tickets
            .AsNoTracking()
            .Where(t =>
                !t.IsDeleted &&
                t.CreatedAt >= startDate &&
                t.CreatedAt < endDate)
            .GroupBy(t => 1)
            .Select(g => new DashboardSummaryResponse
            {
                TotalTickets = g.Count(),
                NewTickets = g.Count(t => t.Status == TicketStatus.New),
                OpenTickets = g.Count(t => t.Status == TicketStatus.Open),
                InProgressTickets = g.Count(t =>
                    t.Status == TicketStatus.InProgress),
                PendingTickets = g.Count(t =>
                    t.Status == TicketStatus.Pending),
                ResolvedTickets = g.Count(t =>
                    t.Status == TicketStatus.Resolved),
                ClosedTickets = g.Count(t =>
                    t.Status == TicketStatus.Closed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result ?? new DashboardSummaryResponse();
    }

    public async Task<SlaPerformanceSummaryResponse> GetSlaPerformanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) = GetDateRange(fromDate, toDate);
        var currentTime = DateTime.UtcNow;

        var query = _context.TicketSlas
            .AsNoTracking()
            .Where(x =>
                !x.Ticket.IsDeleted &&
                x.Ticket.CreatedAt >= startDate &&
                x.Ticket.CreatedAt < endDate);

        var totalTickets = await query.CountAsync(cancellationToken);

        var responseMet = await query.CountAsync(x =>
            x.FirstResponseAt != null && !x.ResponseBreached,
            cancellationToken);

        var responseBreached = await query.CountAsync(x =>
            x.ResponseBreached ||
            (x.FirstResponseAt == null &&
             (x.ResponseDeadline < currentTime ||
              x.Ticket.Status == TicketStatus.Resolved ||
              x.Ticket.Status == TicketStatus.Closed)),
            cancellationToken);

        var responsePending = await query.CountAsync(x =>
            x.FirstResponseAt == null &&
            x.ResponseDeadline >= currentTime &&
            x.Ticket.Status != TicketStatus.Resolved &&
            x.Ticket.Status != TicketStatus.Closed,
            cancellationToken);

        var resolutionMet = await query.CountAsync(x =>
            x.ResolvedAt != null && !x.ResolutionBreached,
            cancellationToken);

        var resolutionBreached = await query.CountAsync(x =>
            x.ResolutionBreached ||
            (x.ResolvedAt == null &&
             x.ResolutionDeadline < currentTime &&
             x.Ticket.Status != TicketStatus.Resolved &&
             x.Ticket.Status != TicketStatus.Closed),
            cancellationToken);

        var resolutionPending = await query.CountAsync(x =>
            x.ResolvedAt == null &&
            x.ResolutionDeadline >= currentTime &&
            x.Ticket.Status != TicketStatus.Resolved &&
            x.Ticket.Status != TicketStatus.Closed,
            cancellationToken);

        var responseEvaluated = responseMet + responseBreached;
        var resolutionEvaluated = resolutionMet + resolutionBreached;

        return new SlaPerformanceSummaryResponse
        {
            TotalTicketsWithSla = totalTickets,

            ResponseMetCount = responseMet,
            ResponseBreachedCount = responseBreached,
            ResponsePendingCount = responsePending,

            ResponseComplianceRate = responseEvaluated == 0
                ? 0
                : Math.Round(
                    responseMet * 100.0 / responseEvaluated, 2),

            ResolutionMetCount = resolutionMet,
            ResolutionBreachedCount = resolutionBreached,
            ResolutionPendingCount = resolutionPending,

            ResolutionComplianceRate = resolutionEvaluated == 0
                ? 0
                : Math.Round(
                    resolutionMet * 100.0 / resolutionEvaluated, 2)
        };
    }

    public async Task<IReadOnlyList<StaffPerformanceResponse>>
        GetStaffPerformanceAsync(
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) = GetDateRange(fromDate, toDate);

        var itStaffEmployeeIds =
            (from user in _context.Users.AsNoTracking()
             join userRole in _context.UserRoles.AsNoTracking()
                 on user.Id equals userRole.UserId
             join role in _context.Roles.AsNoTracking()
                 on userRole.RoleId equals role.Id
             where role.Name == "ITStaff"
                   && user.EmployeeId.HasValue
             select user.EmployeeId!.Value)
            .Distinct();

        var staffData = await _context.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.IsActive &&
                itStaffEmployeeIds.Contains(employee.Id))
            .Select(employee => new
            {
                EmployeeId = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                DepartmentName = employee.Department.Name,

                TotalAssignedTickets = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.AssigneeId == employee.Id),

                ResolvedTickets = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.AssigneeId == employee.Id &&
                    ticket.Status == TicketStatus.Resolved),

                AverageResolutionSeconds = _context.Tickets
                    .Where(ticket =>
                        !ticket.IsDeleted &&
                        ticket.CreatedAt >= startDate &&
                        ticket.CreatedAt < endDate &&
                        ticket.AssigneeId == employee.Id &&
                        ticket.Status == TicketStatus.Resolved &&
                        ticket.TicketSla != null &&
                        ticket.TicketSla.ResolvedAt.HasValue)
                    .Select(ticket => (double?)
                        EF.Functions.DateDiffSecond(
                            ticket.CreatedAt,
                            ticket.TicketSla!.ResolvedAt!.Value))
                    .Average(),

                AverageFirstResponseSeconds = _context.Tickets
                    .Where(ticket =>
                        !ticket.IsDeleted &&
                        ticket.CreatedAt >= startDate &&
                        ticket.CreatedAt < endDate &&
                        ticket.AssigneeId == employee.Id &&
                        ticket.TicketSla != null &&
                        ticket.TicketSla.FirstResponseAt.HasValue)
                    .Select(ticket => (double?)
                        EF.Functions.DateDiffSecond(
                            ticket.CreatedAt,
                            ticket.TicketSla!.FirstResponseAt!.Value))
                    .Average(),

                ResolutionSlaMetCount = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.AssigneeId == employee.Id &&
                    ticket.Status == TicketStatus.Resolved &&
                    ticket.TicketSla != null &&
                    ticket.TicketSla.ResolvedAt.HasValue &&
                    ticket.TicketSla.ResolvedAt.Value <=
                    ticket.TicketSla.ResolutionDeadline),

                ResolutionSlaBreachedCount = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.AssigneeId == employee.Id &&
                    ticket.Status == TicketStatus.Resolved &&
                    ticket.TicketSla != null &&
                    ticket.TicketSla.ResolvedAt.HasValue &&
                    ticket.TicketSla.ResolvedAt.Value >
                    ticket.TicketSla.ResolutionDeadline)
            })
            .ToListAsync(cancellationToken);

        return staffData
            .Select(x =>
            {
                var evaluatedCount =
                    x.ResolutionSlaMetCount +
                    x.ResolutionSlaBreachedCount;

                return new StaffPerformanceResponse
                {
                    EmployeeId = x.EmployeeId,
                    FullName = x.FullName,
                    Email = x.Email,
                    DepartmentName = x.DepartmentName,

                    TotalAssignedTickets = x.TotalAssignedTickets,
                    ResolvedTickets = x.ResolvedTickets,

                    AverageResolutionTimeHours = Math.Round(
                        (x.AverageResolutionSeconds ?? 0) / 3600.0, 2),

                    AverageFirstResponseTimeHours = Math.Round(
                        (x.AverageFirstResponseSeconds ?? 0) / 3600.0, 2),

                    ResolutionSlaMetCount = x.ResolutionSlaMetCount,
                    ResolutionSlaBreachedCount =
                        x.ResolutionSlaBreachedCount,

                    ResolutionSlaComplianceRate = evaluatedCount == 0
                        ? 0
                        : Math.Round(
                            x.ResolutionSlaMetCount * 100.0 /
                            evaluatedCount, 2)
                };
            })
            .OrderByDescending(x => x.TotalAssignedTickets)
            .ThenBy(x => x.FullName)
            .ToList();
    }

    public async Task<IReadOnlyList<DepartmentPerformanceResponse>>
        GetDepartmentPerformanceAsync(
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) = GetDateRange(fromDate, toDate);

        var departmentData = await _context.Departments
            .AsNoTracking()
            .Where(department => department.IsActive)
            .Select(department => new
            {
                DepartmentId = department.Id,
                DepartmentName = department.Name,

                TotalTickets = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.DepartmentId == department.Id),

                AssignedTickets = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.DepartmentId == department.Id &&
                    ticket.AssigneeId != null),

                UnassignedTickets = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.DepartmentId == department.Id &&
                    ticket.AssigneeId == null),

                ResolvedTickets = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.DepartmentId == department.Id &&
                    ticket.Status == TicketStatus.Resolved),

                AverageResolutionSeconds = _context.Tickets
                    .Where(ticket =>
                        !ticket.IsDeleted &&
                        ticket.CreatedAt >= startDate &&
                        ticket.CreatedAt < endDate &&
                        ticket.DepartmentId == department.Id &&
                        ticket.Status == TicketStatus.Resolved &&
                        ticket.TicketSla != null &&
                        ticket.TicketSla.ResolvedAt.HasValue)
                    .Select(ticket => (double?)
                        EF.Functions.DateDiffSecond(
                            ticket.CreatedAt,
                            ticket.TicketSla!.ResolvedAt!.Value))
                    .Average(),

                AverageFirstResponseSeconds = _context.Tickets
                    .Where(ticket =>
                        !ticket.IsDeleted &&
                        ticket.CreatedAt >= startDate &&
                        ticket.CreatedAt < endDate &&
                        ticket.DepartmentId == department.Id &&
                        ticket.TicketSla != null &&
                        ticket.TicketSla.FirstResponseAt.HasValue)
                    .Select(ticket => (double?)
                        EF.Functions.DateDiffSecond(
                            ticket.CreatedAt,
                            ticket.TicketSla!.FirstResponseAt!.Value))
                    .Average(),

                ResolutionSlaMetCount = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.DepartmentId == department.Id &&
                    ticket.Status == TicketStatus.Resolved &&
                    ticket.TicketSla != null &&
                    ticket.TicketSla.ResolvedAt.HasValue &&
                    ticket.TicketSla.ResolvedAt.Value <=
                    ticket.TicketSla.ResolutionDeadline),

                ResolutionSlaBreachedCount = _context.Tickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.CreatedAt >= startDate &&
                    ticket.CreatedAt < endDate &&
                    ticket.DepartmentId == department.Id &&
                    ticket.Status == TicketStatus.Resolved &&
                    ticket.TicketSla != null &&
                    ticket.TicketSla.ResolvedAt.HasValue &&
                    ticket.TicketSla.ResolvedAt.Value >
                    ticket.TicketSla.ResolutionDeadline)
            })
            .ToListAsync(cancellationToken);

        return departmentData
            .Select(x =>
            {
                var evaluatedCount =
                    x.ResolutionSlaMetCount +
                    x.ResolutionSlaBreachedCount;

                return new DepartmentPerformanceResponse
                {
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.DepartmentName,

                    TotalTickets = x.TotalTickets,
                    AssignedTickets = x.AssignedTickets,
                    UnassignedTickets = x.UnassignedTickets,
                    ResolvedTickets = x.ResolvedTickets,

                    AverageResolutionTimeHours = Math.Round(
                        (x.AverageResolutionSeconds ?? 0) / 3600.0, 2),

                    AverageFirstResponseTimeHours = Math.Round(
                        (x.AverageFirstResponseSeconds ?? 0) / 3600.0, 2),

                    ResolutionSlaMetCount = x.ResolutionSlaMetCount,
                    ResolutionSlaBreachedCount =
                        x.ResolutionSlaBreachedCount,

                    ResolutionSlaComplianceRate = evaluatedCount == 0
                        ? 0
                        : Math.Round(
                            x.ResolutionSlaMetCount * 100.0 /
                            evaluatedCount, 2)
                };
            })
            .OrderByDescending(x => x.TotalTickets)
            .ThenBy(x => x.DepartmentName)
            .ToList();
    }
}

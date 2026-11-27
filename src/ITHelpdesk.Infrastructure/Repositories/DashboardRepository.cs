
using ITHelpdesk.Application.DTOs.Dashboard;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Domain.Enums;
using ITHelpdesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITHelpdesk.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _context.Tickets
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .GroupBy(t => 1)
            .Select(g => new DashboardSummaryResponse
            {
                TotalTickets = g.Count(),

                NewTickets = g.Count(t => t.Status == TicketStatus.New),

                OpenTickets = g.Count(t => t.Status == TicketStatus.Open),

                InProgressTickets = g.Count(
                    t => t.Status == TicketStatus.InProgress),

                PendingTickets = g.Count(
                    t => t.Status == TicketStatus.Pending),

                ResolvedTickets = g.Count(
                    t => t.Status == TicketStatus.Resolved),

                ClosedTickets = g.Count(
                    t => t.Status == TicketStatus.Closed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result ?? new DashboardSummaryResponse();
    }

    public async Task<IReadOnlyList<TicketPriorityStatisticsResponse>>
        GetByPriorityAsync(
            CancellationToken cancellationToken = default)
    {
        var groupedData = await _context.Tickets
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .GroupBy(t => t.Priority)
            .Select(g => new
            {
                Priority = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var totalTickets = groupedData.Sum(x => x.Count);

        return Enum.GetValues<TicketPriority>()
            .Select(priority =>
            {
                var count = groupedData
                    .FirstOrDefault(x => x.Priority == priority)?.Count ?? 0;

                return new TicketPriorityStatisticsResponse
                {
                    Priority = priority.ToString(),
                    Count = count,
                    Percentage = totalTickets == 0
                        ? 0
                        : Math.Round(
                            count * 100.0 / totalTickets,
                            2)
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<TicketDepartmentStatisticsResponse>>
        GetByDepartmentAsync(
            CancellationToken cancellationToken = default)
    {
        var groupedData = await _context.Tickets
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .GroupBy(t => new
            {
                t.DepartmentId,
                t.Department.Name
            })
            .Select(g => new
            {
                DepartmentId = g.Key.DepartmentId,
                DepartmentName = g.Key.Name,
                TotalTickets = g.Count()
            })
            .ToListAsync(cancellationToken);

        var totalTickets = groupedData.Sum(x => x.TotalTickets);

        return groupedData
            .Select(x => new TicketDepartmentStatisticsResponse
            {
                DepartmentId = x.DepartmentId,
                DepartmentName = x.DepartmentName,
                TotalTickets = x.TotalTickets,
                Percentage = totalTickets == 0
                    ? 0
                    : Math.Round(
                        x.TotalTickets * 100.0 / totalTickets,
                        2)
            })
            .OrderByDescending(x => x.TotalTickets)
            .ToList();
    }

    public async Task<IReadOnlyList<TicketTrendResponse>> GetTrendsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var startDate = fromDate.ToDateTime(TimeOnly.MinValue);
        var endDate = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var groupedData = await _context.Tickets
            .AsNoTracking()
            .Where(t =>
                !t.IsDeleted &&
                t.CreatedAt >= startDate &&
                t.CreatedAt < endDate)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalTickets = g.Count()
            })
            .ToListAsync(cancellationToken);

        var countByDate = groupedData.ToDictionary(
            x => DateOnly.FromDateTime(x.Date),
            x => x.TotalTickets);

        var result = new List<TicketTrendResponse>();

        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            result.Add(new TicketTrendResponse
            {
                Date = date,
                TotalTickets = countByDate.GetValueOrDefault(date, 0)
            });
        }

        return result;
    }

    public async Task<ReopenRequestSummaryResponse>
    GetReopenRequestSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var requests = _context.TicketReopenRequests
            .AsNoTracking();

        var totalRequests = await requests.CountAsync(cancellationToken);

        var pendingCount = await requests.CountAsync(
            x => x.Status == TicketReopenRequestStatus.Pending,
            cancellationToken);

        var approvedCount = await requests.CountAsync(
            x => x.Status == TicketReopenRequestStatus.Approved,
            cancellationToken);

        var rejectedCount = await requests.CountAsync(
            x => x.Status == TicketReopenRequestStatus.Rejected,
            cancellationToken);

        var reviewedRequests = requests
            .Where(x =>
                x.ReviewedAt != null &&
                (x.Status == TicketReopenRequestStatus.Approved ||
                 x.Status == TicketReopenRequestStatus.Rejected));

        var averageReviewTime = await reviewedRequests
            .Select(x => EF.Functions.DateDiffSecond(
                x.CreatedAt,
                x.ReviewedAt!.Value))
            .AverageAsync(
                cancellationToken);

        return new ReopenRequestSummaryResponse
        {
            TotalRequests = totalRequests,
            PendingCount = pendingCount,
            ApprovedCount = approvedCount,
            RejectedCount = rejectedCount,

            ApprovalRate = totalRequests == 0
                ? 0
                : Math.Round(approvedCount * 100.0 / totalRequests, 2),

            RejectionRate = totalRequests == 0
                ? 0
                : Math.Round(rejectedCount * 100.0 / totalRequests, 2),

            AverageReviewTimeHours = reviewedRequests.Any()
                ? Math.Round(averageReviewTime / 3600.0, 2)
                : 0
        };
    }

    public async Task<IReadOnlyList<ReopenRequestTrendResponse>>
        GetReopenRequestTrendsAsync(
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken = default)
    {
        var startDate = fromDate.ToDateTime(TimeOnly.MinValue);
        var endDate = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var groupedData = await _context.TicketReopenRequests
            .AsNoTracking()
            .Where(x =>
                x.CreatedAt >= startDate &&
                x.CreatedAt < endDate)
            .GroupBy(x => x.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                PendingCount = g.Count(
                    x => x.Status == TicketReopenRequestStatus.Pending),
                ApprovedCount = g.Count(
                    x => x.Status == TicketReopenRequestStatus.Approved),
                RejectedCount = g.Count(
                    x => x.Status == TicketReopenRequestStatus.Rejected)
            })
            .ToListAsync(cancellationToken);

        var countByDate = groupedData.ToDictionary(
            x => DateOnly.FromDateTime(x.Date),
            x => x);

        var result = new List<ReopenRequestTrendResponse>();

        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            countByDate.TryGetValue(date, out var data);

            result.Add(new ReopenRequestTrendResponse
            {
                Date = date,
                PendingCount = data?.PendingCount ?? 0,
                ApprovedCount = data?.ApprovedCount ?? 0,
                RejectedCount = data?.RejectedCount ?? 0,
                TotalRequests = data == null
                    ? 0
                    : data.PendingCount + data.ApprovedCount + data.RejectedCount
            });
        }

        return result;
    }

    public async Task<SlaPerformanceSummaryResponse>
        GetSlaPerformanceSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;

        var query = _context.TicketSlas
            .AsNoTracking()
            .Where(x => !x.Ticket.IsDeleted);

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

    public async Task<IReadOnlyList<SlaPriorityStatisticsResponse>>
        GetSlaStatisticsByPriorityAsync(
        CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;

        var groupedData = await _context.TicketSlas
            .AsNoTracking()
            .Where(x => !x.Ticket.IsDeleted)
            .GroupBy(x => x.Ticket.Priority)
            .Select(g => new
            {
                Priority = g.Key,
                TotalTickets = g.Count(),

                ResponseBreachedCount = g.Count(x =>
                    x.ResponseBreached ||
                    (x.FirstResponseAt == null &&
                     (x.ResponseDeadline < currentTime ||
                      x.Ticket.Status == TicketStatus.Resolved ||
                      x.Ticket.Status == TicketStatus.Closed))),

                ResolutionBreachedCount = g.Count(x =>
                    x.ResolutionBreached ||
                    (x.ResolvedAt == null &&
                     x.ResolutionDeadline < currentTime &&
                     x.Ticket.Status != TicketStatus.Resolved &&
                     x.Ticket.Status != TicketStatus.Closed))
            })
            .ToListAsync(cancellationToken);

        return Enum.GetValues<TicketPriority>()
            .Select(priority =>
            {
                var data = groupedData.FirstOrDefault(
                    x => x.Priority == priority);

                var responseBreached = data?.ResponseBreachedCount ?? 0;
                var resolutionBreached = data?.ResolutionBreachedCount ?? 0;

                return new SlaPriorityStatisticsResponse
                {
                    Priority = priority,
                    TotalTickets = data?.TotalTickets ?? 0,
                    ResponseBreachedCount = responseBreached,
                    ResolutionBreachedCount = resolutionBreached,
                    TotalBreachedCount =
                        responseBreached + resolutionBreached
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<SlaBreachTrendResponse>>
        GetSlaBreachTrendsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;

        var startDate = fromDate.ToDateTime(TimeOnly.MinValue);
        var endDate = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var groupedData = await _context.TicketSlas
            .AsNoTracking()
            .Where(x =>
                !x.Ticket.IsDeleted &&
                x.CreatedAt >= startDate &&
                x.CreatedAt < endDate)
            .GroupBy(x => x.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,

                ResponseBreachedCount = g.Count(x =>
                    x.ResponseBreached ||
                    (x.FirstResponseAt == null &&
                     (x.ResponseDeadline < currentTime ||
                      x.Ticket.Status == TicketStatus.Resolved ||
                      x.Ticket.Status == TicketStatus.Closed))),

                ResolutionBreachedCount = g.Count(x =>
                    x.ResolutionBreached ||
                    (x.ResolvedAt == null &&
                     x.ResolutionDeadline < currentTime &&
                     x.Ticket.Status != TicketStatus.Resolved &&
                     x.Ticket.Status != TicketStatus.Closed))
            })
            .ToListAsync(cancellationToken);

        var countByDate = groupedData.ToDictionary(
            x => DateOnly.FromDateTime(x.Date),
            x => x);

        var result = new List<SlaBreachTrendResponse>();

        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            countByDate.TryGetValue(date, out var data);

            var responseBreached = data?.ResponseBreachedCount ?? 0;
            var resolutionBreached = data?.ResolutionBreachedCount ?? 0;

            result.Add(new SlaBreachTrendResponse
            {
                Date = date,
                ResponseBreachedCount = responseBreached,
                ResolutionBreachedCount = resolutionBreached,
                TotalBreachedCount =
                    responseBreached + resolutionBreached
            });
        }

        return result;
    }


    public async Task<TicketAgingStatisticsResponse>
        GetTicketAgingStatisticsAsync(
            CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;

        var oneDayAgo = currentTime.AddDays(-1);
        var threeDaysAgo = currentTime.AddDays(-3);
        var sevenDaysAgo = currentTime.AddDays(-7);

        var activeTickets = _context.Tickets
            .AsNoTracking()
            .Where(t =>
                !t.IsDeleted &&
                t.Status != TicketStatus.Resolved &&
                t.Status != TicketStatus.Closed);

        return await activeTickets
            .GroupBy(t => 1)
            .Select(g => new TicketAgingStatisticsResponse
            {
                TotalOpenTickets = g.Count(),

                FromZeroToOneDay = g.Count(t =>
                    t.CreatedAt >= oneDayAgo),

                FromOneToThreeDays = g.Count(t =>
                    t.CreatedAt < oneDayAgo &&
                    t.CreatedAt >= threeDaysAgo),

                FromThreeToSevenDays = g.Count(t =>
                    t.CreatedAt < threeDaysAgo &&
                    t.CreatedAt >= sevenDaysAgo),

                OverSevenDays = g.Count(t =>
                    t.CreatedAt < sevenDaysAgo)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new TicketAgingStatisticsResponse();
    }


    public async Task<UnassignedTicketStatisticsResponse>
        GetUnassignedTicketStatisticsAsync(
            CancellationToken cancellationToken = default)
    {
        var groupedData = await _context.Tickets
            .AsNoTracking()
            .Where(t =>
                !t.IsDeleted &&
                t.AssigneeId == null &&
                t.Status != TicketStatus.Resolved &&
                t.Status != TicketStatus.Closed)
            .GroupBy(t => t.Priority)
            .Select(g => new UnassignedTicketByPriorityResponse
            {
                Priority = g.Key,
                TicketCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        var result = new UnassignedTicketStatisticsResponse
        {
            TotalUnassignedTickets = groupedData.Sum(x => x.TicketCount),
            ByPriority = Enum.GetValues<TicketPriority>()
                .Select(priority => new UnassignedTicketByPriorityResponse
                {
                    Priority = priority,
                    TicketCount = groupedData
                        .FirstOrDefault(x => x.Priority == priority)
                        ?.TicketCount ?? 0
                })
                .ToList()
        };

        result.LowPriorityCount = result.ByPriority
            .First(x => x.Priority == TicketPriority.Low).TicketCount;

        result.MediumPriorityCount = result.ByPriority
            .First(x => x.Priority == TicketPriority.Medium).TicketCount;

        result.HighPriorityCount = result.ByPriority
            .First(x => x.Priority == TicketPriority.High).TicketCount;

        result.CriticalPriorityCount = result.ByPriority
            .First(x => x.Priority == TicketPriority.Critical).TicketCount;

        return result;
    }


    public async Task<IReadOnlyList<StaffWorkloadResponse>>
        GetStaffWorkloadAsync(
            CancellationToken cancellationToken = default)
    {
        var itStaffEmployeeIds =
            (from user in _context.Users.AsNoTracking()
             join userRole in _context.UserRoles.AsNoTracking()
                 on user.Id equals userRole.UserId
             join role in _context.Roles.AsNoTracking()
                 on userRole.RoleId equals role.Id
             where role.Name == "ITStaff"
                   && user.EmployeeId.HasValue
             select user.EmployeeId ?? Guid.Empty)
            .Distinct();

        var result = await _context.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.IsActive &&
                itStaffEmployeeIds.Contains(employee.Id))
            .Select(employee => new StaffWorkloadResponse
            {
                EmployeeId = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,

                TotalAssignedTickets = employee.AssignedTickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.Status != TicketStatus.Resolved &&
                    ticket.Status != TicketStatus.Closed),

                NewTickets = employee.AssignedTickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.Status == TicketStatus.New),

                OpenTickets = employee.AssignedTickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.Status == TicketStatus.Open),

                InProgressTickets = employee.AssignedTickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.Status == TicketStatus.InProgress),

                PendingTickets = employee.AssignedTickets.Count(ticket =>
                    !ticket.IsDeleted &&
                    ticket.Status == TicketStatus.Pending)
            })
            .OrderByDescending(x => x.TotalAssignedTickets)
            .ThenBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return result;
    }


    public async Task<OverdueTicketStatisticsResponse>
        GetOverdueTicketStatisticsAsync(
            CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;

        var query = _context.TicketSlas
            .AsNoTracking()
            .Where(x => !x.Ticket.IsDeleted);

        var responseBreachedQuery = query.Where(x =>
            x.ResponseBreached ||
            (x.FirstResponseAt == null &&
             (x.ResponseDeadline < currentTime ||
              x.Ticket.Status == TicketStatus.Resolved ||
              x.Ticket.Status == TicketStatus.Closed)));

        var resolutionBreachedQuery = query.Where(x =>
            x.ResolutionBreached ||
            (x.ResolvedAt == null &&
             x.ResolutionDeadline < currentTime &&
             x.Ticket.Status != TicketStatus.Resolved &&
             x.Ticket.Status != TicketStatus.Closed));

        var responseBreachedCount =
            await responseBreachedQuery.CountAsync(cancellationToken);

        var resolutionBreachedCount =
            await resolutionBreachedQuery.CountAsync(cancellationToken);

        var totalBreachedTickets = await query
            .Where(x =>
                x.ResponseBreached ||
                (x.FirstResponseAt == null &&
                 (x.ResponseDeadline < currentTime ||
                  x.Ticket.Status == TicketStatus.Resolved ||
                  x.Ticket.Status == TicketStatus.Closed)) ||
                x.ResolutionBreached ||
                (x.ResolvedAt == null &&
                 x.ResolutionDeadline < currentTime &&
                 x.Ticket.Status != TicketStatus.Resolved &&
                 x.Ticket.Status != TicketStatus.Closed))
            .Select(x => x.TicketId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalTicketsWithSla =
            await query.CountAsync(cancellationToken);

        return new OverdueTicketStatisticsResponse
        {
            TotalTicketsWithSla = totalTicketsWithSla,
            ResponseBreachedCount = responseBreachedCount,
            ResolutionBreachedCount = resolutionBreachedCount,
            TotalBreachedTickets = totalBreachedTickets
        };
    }


    public async Task<IReadOnlyList<TicketStatusDistributionResponse>>
        GetTicketStatusDistributionAsync(
            CancellationToken cancellationToken = default)
    {
        var groupedData = await _context.Tickets
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .GroupBy(t => t.Status)
            .Select(g => new
            {
                Status = g.Key,
                TicketCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        var totalTickets = groupedData.Sum(x => x.TicketCount);

        return Enum.GetValues<TicketStatus>()
            .Select(status =>
            {
                var count = groupedData
                    .FirstOrDefault(x => x.Status == status)
                    ?.TicketCount ?? 0;

                return new TicketStatusDistributionResponse
                {
                    Status = status,
                    TicketCount = count,
                    Percentage = totalTickets == 0
                        ? 0
                        : Math.Round(
                            count * 100.0 / totalTickets,
                            2)
                };
            })
            .ToList();
    }


    public async Task<IReadOnlyList<StaffPerformanceResponse>>
        GetStaffPerformanceAsync(
            CancellationToken cancellationToken = default)
    {
        // Get active employees whose accounts have the ITStaff role.
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
                DepartmentName = employee.Department.Name
            })
            .ToListAsync(cancellationToken);

        if (staffData.Count == 0)
        {
            return new List<StaffPerformanceResponse>();
        }

        // Aggregate tickets once per assigned employee.
        var staffIds = staffData
            .Select(employee => employee.EmployeeId)
            .ToArray();

        var ticketStats = await _context.Tickets
            .AsNoTracking()
            .Where(ticket =>
                !ticket.IsDeleted &&
                ticket.AssigneeId.HasValue &&
                staffIds.Contains(ticket.AssigneeId.Value))
            .GroupBy(ticket => ticket.AssigneeId!.Value)
            .Select(group => new
            {
                EmployeeId = group.Key,

                TotalAssignedTickets = group.Count(),

                ResolvedTickets = group.Count(ticket =>
                    ticket.Status == TicketStatus.Resolved),

                AverageResolutionSeconds = group
                    .Where(ticket =>
                        ticket.Status == TicketStatus.Resolved &&
                        ticket.TicketSla != null &&
                        ticket.TicketSla.ResolvedAt.HasValue)
                    .Select(ticket => (double?)
                        EF.Functions.DateDiffSecond(
                            ticket.CreatedAt,
                            ticket.TicketSla!.ResolvedAt!.Value))
                    .Average(),

                AverageFirstResponseSeconds = group
                    .Where(ticket =>
                        ticket.TicketSla != null &&
                        ticket.TicketSla.FirstResponseAt.HasValue)
                    .Select(ticket => (double?)
                        EF.Functions.DateDiffSecond(
                            ticket.CreatedAt,
                            ticket.TicketSla!.FirstResponseAt!.Value))
                    .Average(),

                ResolutionSlaMetCount = group.Count(ticket =>
                    ticket.Status == TicketStatus.Resolved &&
                    ticket.TicketSla != null &&
                    ticket.TicketSla.ResolvedAt.HasValue &&
                    ticket.TicketSla.ResolvedAt.Value <=
                    ticket.TicketSla.ResolutionDeadline),

                ResolutionSlaBreachedCount = group.Count(ticket =>
                    ticket.Status == TicketStatus.Resolved &&
                    ticket.TicketSla != null &&
                    ticket.TicketSla.ResolvedAt.HasValue &&
                    ticket.TicketSla.ResolvedAt.Value >
                    ticket.TicketSla.ResolutionDeadline)
            })
            .ToDictionaryAsync(
                x => x.EmployeeId,
                cancellationToken);

        // Combine employee information with aggregated ticket statistics.
        return staffData
            .Select(employee =>
            {
                ticketStats.TryGetValue(
                    employee.EmployeeId,
                    out var stats);

                var totalAssignedTickets =
                    stats?.TotalAssignedTickets ?? 0;

                var resolvedTickets =
                    stats?.ResolvedTickets ?? 0;

                var resolutionSlaMetCount =
                    stats?.ResolutionSlaMetCount ?? 0;

                var resolutionSlaBreachedCount =
                    stats?.ResolutionSlaBreachedCount ?? 0;

                var evaluatedCount =
                    resolutionSlaMetCount +
                    resolutionSlaBreachedCount;

                return new StaffPerformanceResponse
                {
                    EmployeeId = employee.EmployeeId,
                    FullName = employee.FullName,
                    Email = employee.Email,
                    DepartmentName = employee.DepartmentName,

                    TotalAssignedTickets = totalAssignedTickets,
                    ResolvedTickets = resolvedTickets,

                    AverageResolutionTimeHours = Math.Round(
                        (stats?.AverageResolutionSeconds ?? 0) / 3600.0,
                        2),

                    AverageFirstResponseTimeHours = Math.Round(
                        (stats?.AverageFirstResponseSeconds ?? 0) / 3600.0,
                        2),

                    ResolutionSlaMetCount = resolutionSlaMetCount,
                    ResolutionSlaBreachedCount =
                        resolutionSlaBreachedCount,

                    ResolutionSlaComplianceRate = evaluatedCount == 0
                        ? 0
                        : Math.Round(
                            resolutionSlaMetCount * 100.0 /
                            evaluatedCount,
                            2)
                };
            })
            .OrderByDescending(x => x.TotalAssignedTickets)
            .ThenBy(x => x.FullName)
            .ToList();
    }




    public async Task<IReadOnlyList<DepartmentPerformanceResponse>>
        GetDepartmentPerformanceAsync(
            CancellationToken cancellationToken = default)
    {
        // Get active departments.
        var departmentData = await _context.Departments
            .AsNoTracking()
            .Where(department => department.IsActive)
            .Select(department => new
            {
                DepartmentId = department.Id,
                DepartmentName = department.Name
            })
            .ToListAsync(cancellationToken);

        if (departmentData.Count == 0)
        {
            return new List<DepartmentPerformanceResponse>();
        }

        // Aggregate tickets once per department.
        var departmentIds = departmentData
            .Select(department => department.DepartmentId)
            .ToArray();

        var ticketStats = await _context.Tickets
            .AsNoTracking()
            .Where(ticket =>
                !ticket.IsDeleted &&
                departmentIds.Contains(ticket.DepartmentId))
            .GroupBy(ticket => ticket.DepartmentId)
            .Select(group => new
            {
                DepartmentId = group.Key,

                TotalTickets = group.Count(),

                AssignedTickets = group.Count(ticket =>
                    ticket.AssigneeId != null),

                UnassignedTickets = group.Count(ticket =>
                    ticket.AssigneeId == null),

                ResolvedTickets = group.Count(ticket =>
                    ticket.Status == TicketStatus.Resolved),

                AverageResolutionSeconds = group
                    .Where(ticket =>
                        ticket.Status == TicketStatus.Resolved &&
                        ticket.TicketSla != null &&
                        ticket.TicketSla.ResolvedAt.HasValue)
                    .Select(ticket => (double?)
                        EF.Functions.DateDiffSecond(
                            ticket.CreatedAt,
                            ticket.TicketSla!.ResolvedAt!.Value))
                    .Average(),

                AverageFirstResponseSeconds = group
                    .Where(ticket =>
                        ticket.TicketSla != null &&
                        ticket.TicketSla.FirstResponseAt.HasValue)
                    .Select(ticket => (double?)
                        EF.Functions.DateDiffSecond(
                            ticket.CreatedAt,
                            ticket.TicketSla!.FirstResponseAt!.Value))
                    .Average(),

                ResolutionSlaMetCount = group.Count(ticket =>
                    ticket.Status == TicketStatus.Resolved &&
                    ticket.TicketSla != null &&
                    ticket.TicketSla.ResolvedAt.HasValue &&
                    ticket.TicketSla.ResolvedAt.Value <=
                    ticket.TicketSla.ResolutionDeadline),

                ResolutionSlaBreachedCount = group.Count(ticket =>
                    ticket.Status == TicketStatus.Resolved &&
                    ticket.TicketSla != null &&
                    ticket.TicketSla.ResolvedAt.HasValue &&
                    ticket.TicketSla.ResolvedAt.Value >
                    ticket.TicketSla.ResolutionDeadline)
            })
            .ToDictionaryAsync(
                x => x.DepartmentId,
                cancellationToken);

        // Combine department information with ticket statistics.
        return departmentData
            .Select(department =>
            {
                ticketStats.TryGetValue(
                    department.DepartmentId,
                    out var stats);

                var totalTickets = stats?.TotalTickets ?? 0;
                var assignedTickets = stats?.AssignedTickets ?? 0;
                var unassignedTickets = stats?.UnassignedTickets ?? 0;
                var resolvedTickets = stats?.ResolvedTickets ?? 0;

                var resolutionSlaMetCount =
                    stats?.ResolutionSlaMetCount ?? 0;

                var resolutionSlaBreachedCount =
                    stats?.ResolutionSlaBreachedCount ?? 0;

                var evaluatedCount =
                    resolutionSlaMetCount +
                    resolutionSlaBreachedCount;

                return new DepartmentPerformanceResponse
                {
                    DepartmentId = department.DepartmentId,
                    DepartmentName = department.DepartmentName,

                    TotalTickets = totalTickets,
                    AssignedTickets = assignedTickets,
                    UnassignedTickets = unassignedTickets,
                    ResolvedTickets = resolvedTickets,

                    AverageResolutionTimeHours = Math.Round(
                        (stats?.AverageResolutionSeconds ?? 0) / 3600.0,
                        2),

                    AverageFirstResponseTimeHours = Math.Round(
                        (stats?.AverageFirstResponseSeconds ?? 0) / 3600.0,
                        2),

                    ResolutionSlaMetCount = resolutionSlaMetCount,
                    ResolutionSlaBreachedCount =
                        resolutionSlaBreachedCount,

                    ResolutionSlaComplianceRate = evaluatedCount == 0
                        ? 0
                        : Math.Round(
                            resolutionSlaMetCount * 100.0 /
                            evaluatedCount,
                            2)
                };
            })
            .OrderByDescending(x => x.TotalTickets)
            .ThenBy(x => x.DepartmentName)
            .ToList();
    }

}


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
}

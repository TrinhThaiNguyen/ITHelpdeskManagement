
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ITHelpdeshMangament.Models.Dashboard.Dtos;

namespace ITHelpdeshMangament.Services.Dashboard;

public class DashboardApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DashboardApiService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<T?> GetAsync<T>(
        string url,
        CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?
            .Session.GetString("AccessToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Authentication session has expired.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to access dashboard data.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(
            cancellationToken: cancellationToken);
    }

    private async Task<byte[]> DownloadFileAsync(
        string url,
        CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?
            .Session.GetString("AccessToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Authentication session has expired.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to download this report.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(
            cancellationToken);
    }

    public Task<DashboardSummaryDto?> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<DashboardSummaryDto>(
            "api/dashboard/summary",
            cancellationToken);
    }

    public async Task<List<TicketPriorityStatisticsDto>> GetByPriorityAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<TicketPriorityStatisticsDto>>(
            "api/dashboard/by-priority",
            cancellationToken) ?? new();
    }

    public async Task<List<TicketDepartmentStatisticsDto>> GetByDepartmentAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<TicketDepartmentStatisticsDto>>(
            "api/dashboard/by-department",
            cancellationToken) ?? new();
    }

    public async Task<List<TicketTrendDto>> GetTrendsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"api/dashboard/trends?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";

        return await GetAsync<List<TicketTrendDto>>(
            url,
            cancellationToken) ?? new();
    }

    public Task<SlaPerformanceSummaryDto?> GetSlaSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<SlaPerformanceSummaryDto>(
            "api/dashboard/sla/summary",
            cancellationToken);
    }

    public async Task<List<TicketStatusDistributionDto>>
        GetTicketStatusDistributionAsync(
            CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<TicketStatusDistributionDto>>(
            "api/dashboard/ticket-status-distribution",
            cancellationToken) ?? new();
    }

    public async Task<ReopenRequestSummaryDto> GetReopenRequestSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<ReopenRequestSummaryDto>(
            "api/dashboard/reopen-requests/summary",
            cancellationToken) ?? new();
    }

    public async Task<List<ReopenRequestTrendDto>> GetReopenRequestTrendsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/dashboard/reopen-requests/trends?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";

        return await GetAsync<List<ReopenRequestTrendDto>>(url, cancellationToken) ?? new();
    }

    public async Task<List<SlaPriorityStatisticsDto>> GetSlaPriorityStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<SlaPriorityStatisticsDto>>(
            "api/dashboard/sla/by-priority",
            cancellationToken) ?? new();
    }

    public async Task<List<SlaBreachTrendDto>> GetSlaBreachTrendsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/dashboard/sla/trends?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";

        return await GetAsync<List<SlaBreachTrendDto>>(url, cancellationToken) ?? new();
    }

    public async Task<TicketAgingStatisticsDto> GetTicketAgingAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<TicketAgingStatisticsDto>(
            "api/dashboard/ticket-aging",
            cancellationToken) ?? new();
    }

    public async Task<UnassignedTicketStatisticsDto> GetUnassignedTicketsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<UnassignedTicketStatisticsDto>(
            "api/dashboard/unassigned-tickets",
            cancellationToken) ?? new();
    }

    public async Task<List<StaffWorkloadDto>> GetStaffWorkloadAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<StaffWorkloadDto>>(
            "api/dashboard/staff-workload",
            cancellationToken) ?? new();
    }

    public async Task<OverdueTicketStatisticsDto> GetOverdueTicketsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<OverdueTicketStatisticsDto>(
            "api/dashboard/overdue-tickets",
            cancellationToken) ?? new();
    }

    public async Task<List<StaffPerformanceDto>> GetStaffPerformanceAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<StaffPerformanceDto>>(
            "api/dashboard/staff-performance",
            cancellationToken) ?? new();
    }

    public async Task<List<DepartmentPerformanceDto>> GetDepartmentPerformanceAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<DepartmentPerformanceDto>>(
            "api/dashboard/department-performance",
            cancellationToken) ?? new();
    }

    public Task<byte[]> ExportSummaryAsync(
    DateOnly fromDate,
    DateOnly toDate,
    CancellationToken cancellationToken = default)
    {
        var url =
            $"api/dashboard/export/summary?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";

        return DownloadFileAsync(url, cancellationToken);
    }

    public Task<byte[]> ExportSlaPerformanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"api/dashboard/export/sla-performance?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";

        return DownloadFileAsync(url, cancellationToken);
    }

    public Task<byte[]> ExportStaffPerformanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"api/dashboard/export/staff-performance?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";

        return DownloadFileAsync(url, cancellationToken);
    }

    public Task<byte[]> ExportDepartmentPerformanceAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"api/dashboard/export/department-performance?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";

        return DownloadFileAsync(url, cancellationToken);
    }

}

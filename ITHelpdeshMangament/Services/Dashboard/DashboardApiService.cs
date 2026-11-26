
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
}

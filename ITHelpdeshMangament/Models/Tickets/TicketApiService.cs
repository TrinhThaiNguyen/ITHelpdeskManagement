
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ITHelpdeshMangament.Models.Tickets;

namespace ITHelpdeshMangament.Services.Tickets;

public class TicketApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public TicketApiService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TicketListViewModel> GetTicketsAsync(
        TicketListViewModel filters,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"pageNumber={filters.PageNumber}",
            $"pageSize={filters.PageSize}",
            "sortBy=createdAt",
            "sortDescending=true"
        };

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            query.Add(
                $"searchTerm={Uri.EscapeDataString(filters.SearchTerm)}");
        }

        if (filters.Status.HasValue)
        {
            query.Add($"status={filters.Status.Value}");
        }

        if (filters.Priority.HasValue)
        {
            query.Add($"priority={filters.Priority.Value}");
        }

        var url = $"api/tickets?{string.Join("&", query)}";

        var result = await GetAsync<PagedTicketResponse>(
            url,
            cancellationToken);

        return new TicketListViewModel
        {
            Items = result?.Items.Select(ticket => new TicketItemViewModel
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CategoryName = ticket.CategoryName,
                DepartmentName = ticket.DepartmentName,
                RequesterId = ticket.RequesterId,
                AssigneeId = ticket.AssigneeId,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt
            }).ToList() ?? [],

            PageNumber = result?.PageNumber ?? filters.PageNumber,
            PageSize = result?.PageSize ?? filters.PageSize,
            TotalCount = result?.TotalCount ?? 0,
            SearchTerm = filters.SearchTerm,
            Status = filters.Status,
            Priority = filters.Priority
        };
    }


    public async Task<TicketDetailsViewModel?> GetTicketByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Ticket ID cannot be empty.",
                nameof(id));
        }

        var result = await GetAsync<TicketDetailsViewModel>(
            $"api/tickets/{id}",
            cancellationToken);

        return result;
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

        if (response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to access ticket data.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(
            JsonOptions,
            cancellationToken);
    }

    private sealed class PagedTicketResponse
    {
        public List<TicketApiResponse> Items { get; set; } = [];

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }
    }

    private sealed class TicketApiResponse
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Status { get; set; }

        public int Priority { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public Guid RequesterId { get; set; }

        public Guid? AssigneeId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

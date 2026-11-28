
using ITHelpdeshMangament.Models.Tickets;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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

        if (filters.DepartmentId.HasValue)
        {
            query.Add($"departmentId={filters.DepartmentId.Value}");
        }

        if (filters.AssigneeId.HasValue)
        {
            query.Add($"assigneeId={filters.AssigneeId.Value}");
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
            Priority = filters.Priority,
            DepartmentId = filters.DepartmentId,
            AssigneeId = filters.AssigneeId
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


    public async Task<List<TicketCommentViewModel>> GetTicketCommentsAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException(
                "Ticket ID cannot be empty.",
                nameof(ticketId));
        }

        return await GetAsync<List<TicketCommentViewModel>>(
            $"api/tickets/{ticketId}/comments",
            cancellationToken) ?? [];
    }

    public async Task<List<TicketHistoryViewModel>> GetTicketHistoryAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException(
                "Ticket ID cannot be empty.",
                nameof(ticketId));
        }

        return await GetAsync<List<TicketHistoryViewModel>>(
            $"api/tickets/{ticketId}/history",
            cancellationToken) ?? [];
    }

    public async Task AddTicketCommentAsync(
        Guid ticketId,
        string content,
        bool isInternal,
        CancellationToken cancellationToken = default)
    {
        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException(
                "Ticket ID cannot be empty.",
                nameof(ticketId));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Comment content is required.",
                nameof(content));
        }

        var token = _httpContextAccessor.HttpContext?
            .Session.GetString("AccessToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Authentication session has expired.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/tickets/{ticketId}/comments");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Content = JsonContent.Create(new
        {
            Content = content.Trim(),
            IsInternal = isInternal
        });

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to add comments.");
        }

        response.EnsureSuccessStatusCode();
    }


    public async Task<List<SelectListItem>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await GetAsync<List<LookupApiResponse>>(
            "api/ticket-categories",
            cancellationToken);

        return categories?
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToList() ?? [];
    }

    public async Task<List<SelectListItem>> GetDepartmentsAsync(
        CancellationToken cancellationToken = default)
    {
        var departments = await GetAsync<List<LookupApiResponse>>(
            "api/departments",
            cancellationToken);

        return departments?
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToList() ?? [];
    }

    public async Task UpdateTicketAsync(
        EditTicketViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.Id == Guid.Empty)
            throw new ArgumentException("Ticket ID cannot be empty.");

        var requestBody = new
        {
            model.Title,
            model.Description,
            model.Priority,
            model.CategoryId,
            model.DepartmentId
        };

        await SendAsync(
            HttpMethod.Put,
            $"api/tickets/{model.Id}",
            requestBody,
            cancellationToken);
    }

    public async Task UpdateTicketStatusAsync(
        Guid ticketId,
        int status,
        CancellationToken cancellationToken = default)
    {
        if (ticketId == Guid.Empty)
            throw new ArgumentException("Ticket ID cannot be empty.");

        await SendAsync(
            HttpMethod.Patch,
            $"api/tickets/{ticketId}/status",
            new { Status = status },
            cancellationToken);
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

    private async Task SendAsync<T>(
        HttpMethod method,
        string url,
        T body,
        CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?
            .Session.GetString("AccessToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Authentication session has expired.");
        }

        using var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Content = JsonContent.Create(body);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to update this ticket.");
        }

        response.EnsureSuccessStatusCode();
    }

    private sealed class LookupApiResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
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

    public async Task<List<TicketAssigneeViewModel>> GetActiveITStaffAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<TicketAssigneeViewModel>>(
            "api/employees/it-staff",
            cancellationToken) ?? [];
    }

    public async Task<TicketAssignmentViewModel?> GetAssignmentAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<TicketAssignmentViewModel>(
            $"api/tickets/{ticketId}/assignment",
            cancellationToken);
    }

    public async Task AssignTicketAsync(
        Guid ticketId,
        AssignTicketRequestViewModel model,
        CancellationToken cancellationToken = default)
    {
        await SendAsync(
            HttpMethod.Put,
            $"api/tickets/{ticketId}/assignment",
            model,
            cancellationToken);
    }

    public async Task UnassignTicketAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        await SendAsync(
            HttpMethod.Delete,
            $"api/tickets/{ticketId}/assignment",
            new { },
            cancellationToken);
    }
}

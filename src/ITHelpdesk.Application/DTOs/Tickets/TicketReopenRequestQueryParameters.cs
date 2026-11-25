
using ITHelpdesk.Domain.Enums;

namespace ITHelpdesk.Application.DTOs.Tickets;

public class TicketReopenRequestQueryParameters
{
    // Pagination
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    // Search
    public string? SearchTerm { get; set; }

    // Filtering
    public TicketReopenRequestStatus? Status { get; set; }

    // Sorting
    public string SortBy { get; set; } = "createdAt";

    public bool SortDescending { get; set; } = true;
}

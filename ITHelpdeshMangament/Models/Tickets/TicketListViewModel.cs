
namespace ITHelpdeshMangament.Models.Tickets;

public class TicketListViewModel
{
    public List<TicketItemViewModel> Items { get; set; } = [];

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalCount { get; set; }

    public int TotalPages =>
        (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public string? SearchTerm { get; set; }

    public int? Status { get; set; }

    public int? Priority { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? AssigneeId { get; set; }
}

public class TicketItemViewModel
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

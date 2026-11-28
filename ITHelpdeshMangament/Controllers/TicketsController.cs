
using ITHelpdeshMangament.Models.Tickets;
using ITHelpdeshMangament.Services.Tickets;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ITHelpdeshMangament.Controllers;

public class TicketsController : Controller
{
    private readonly TicketApiService _ticketApiService;

    public TicketsController(TicketApiService ticketApiService)
    {
        _ticketApiService = ticketApiService;
    }

    private bool IsAuthorized()
    {
        var token = HttpContext.Session.GetString("AccessToken");
        var expiresAt = HttpContext.Session.GetString("ExpiresAtUtc");
        var roles = HttpContext.Session.GetString("Roles");

        if (string.IsNullOrWhiteSpace(token) ||
            !DateTime.TryParse(
                expiresAt,
                null,
                DateTimeStyles.RoundtripKind,
                out var expiration) ||
            expiration <= DateTime.UtcNow)
        {
            HttpContext.Session.Clear();
            return false;
        }

        var isAuthorized = (roles ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(role =>
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("ITStaff", StringComparison.OrdinalIgnoreCase));

        return isAuthorized;
    }

    public async Task<IActionResult> Index(
        string? searchTerm,
        int? status,
        int? priority,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        var filters = new TicketListViewModel
        {
            SearchTerm = searchTerm,
            Status = status,
            Priority = priority,
            PageNumber = Math.Max(1, pageNumber),
            PageSize = Math.Clamp(pageSize, 5, 50)
        };

        try
        {
            var result = await _ticketApiService.GetTicketsAsync(
                filters,
                cancellationToken);

            return View(result);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the ticket service. Please try again later.";

            return View(filters);
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] =
                "The ticket service request timed out.";

            return View(filters);
        }
    }


    public async Task<IActionResult> Details(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (id == Guid.Empty)
        {
            return NotFound();
        }

        try
        {
            var ticket = await _ticketApiService.GetTicketByIdAsync(
                id,
                cancellationToken);

            if (ticket is null)
            {
                return NotFound();
            }

            ticket.Comments = await _ticketApiService.GetTicketCommentsAsync(
                id,
                cancellationToken);

            ticket.Histories = await _ticketApiService.GetTicketHistoryAsync(
                id,
                cancellationToken);
            var assignment = await _ticketApiService.GetAssignmentAsync(
                id,
                cancellationToken);

            ticket.AssigneeId = assignment?.AssigneeId;
            ticket.AssigneeName = assignment?.AssigneeName;
            ticket.AssigneeEmail = assignment?.AssigneeEmail;
            ticket.AssignedAt = assignment?.AssignedAt;

            ticket.AvailableAssignees = await _ticketApiService.GetActiveITStaffAsync(
                cancellationToken);

            // Resolve assignee IDs to display names in activity history.
            foreach (var history in ticket.Histories)
            {
                history.OldValueDisplay = Guid.TryParse(history.OldValue, out var oldId)
                    ? ticket.AvailableAssignees.FirstOrDefault(x => x.Id == oldId)?.FullName
                        ?? history.OldValue
                    : history.OldValue;

                history.NewValueDisplay = Guid.TryParse(history.NewValue, out var newId)
                    ? ticket.AvailableAssignees.FirstOrDefault(x => x.Id == newId)?.FullName
                        ?? history.NewValue
                    : history.NewValue;
            }

            return View(ticket);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to load ticket details. Please try again later.";

            return RedirectToAction(nameof(Index));
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] =
                "The ticket service request timed out.";

            return RedirectToAction(nameof(Index));
        }
    }


    [HttpGet]
    public async Task<IActionResult> Edit(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (id == Guid.Empty)
        {
            return NotFound();
        }

        try
        {
            var ticket = await _ticketApiService.GetTicketByIdAsync(
                id,
                cancellationToken);

            if (ticket is null)
            {
                return NotFound();
            }

            var model = new EditTicketViewModel
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority,
                CategoryId = ticket.CategoryId,
                DepartmentId = ticket.DepartmentId
            };

            await LoadEditDropdownsAsync(model, cancellationToken);

            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to load ticket for editing.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] =
                "The ticket service request timed out.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditTicketViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (model.Id == Guid.Empty)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadEditDropdownsAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            await _ticketApiService.UpdateTicketAsync(
                model,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Ticket updated successfully.";

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to update ticket. Please check the information and try again.";

            await LoadEditDropdownsAsync(model, cancellationToken);
            return View(model);
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] =
                "The ticket update request timed out.";

            await LoadEditDropdownsAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        int status,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (id == Guid.Empty)
        {
            return NotFound();
        }

        try
        {
            await _ticketApiService.UpdateTicketStatusAsync(
                id,
                status,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Ticket status updated successfully.";
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to change ticket status. The requested transition may not be allowed.";
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] =
                "The status update request timed out.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadEditDropdownsAsync(
        EditTicketViewModel model,
        CancellationToken cancellationToken)
    {
        model.Categories = await _ticketApiService.GetCategoriesAsync(
            cancellationToken);

        model.Departments = await _ticketApiService.GetDepartmentsAsync(
            cancellationToken);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(
        Guid id,
        string content,
        bool isInternal = false,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (id == Guid.Empty)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Comment content is required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            await _ticketApiService.AddTicketCommentAsync(
                id,
                content,
                isInternal,
                cancellationToken);

            TempData["SuccessMessage"] = "Comment added successfully.";
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to add comment. Please try again later.";
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] =
                "The comment request timed out.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(
    Guid id,
    AssignTicketRequestViewModel model,
    CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (id == Guid.Empty || model.AssigneeId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Please select a valid IT Staff member.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            await _ticketApiService.AssignTicketAsync(
                id,
                model,
                cancellationToken);

            TempData["SuccessMessage"] = "Ticket assigned successfully.";
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = "Unable to assign ticket.";
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] = "The assignment request timed out.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unassign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized())
        {
            return RedirectToAction("Login", "Account");
        }

        if (id == Guid.Empty)
        {
            return NotFound();
        }

        try
        {
            await _ticketApiService.UnassignTicketAsync(
                id,
                cancellationToken);

            TempData["SuccessMessage"] = "Ticket unassigned successfully.";
        }
        catch (UnauthorizedAccessException)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = "Unable to unassign ticket.";
        }
        catch (TaskCanceledException)
        {
            TempData["ErrorMessage"] = "The unassignment request timed out.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

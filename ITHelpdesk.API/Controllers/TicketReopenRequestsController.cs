
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITHelpdesk.API.Controllers;

[ApiController]
[Route("api/ticket-reopen-requests")]
[Authorize]
public class TicketReopenRequestsController : ControllerBase
{
    private readonly ITicketReopenRequestService _service;

    public TicketReopenRequestsController(
        ITicketReopenRequestService service)
    {
        _service = service;
    }

    // POST: api/ticket-reopen-requests/tickets/{ticketId}
    [HttpPost("tickets/{ticketId:guid}")]
    [Authorize(Roles = "Requester")]
    public async Task<IActionResult> Create(
        Guid ticketId,
        [FromBody] CreateTicketReopenRequest request)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub")?.Value;

        var employeeIdClaim = User.FindFirst("employee_id")?.Value;

        if (!Guid.TryParse(userIdClaim, out var requesterUserId)
            || requesterUserId == Guid.Empty
            || !Guid.TryParse(employeeIdClaim, out var requesterEmployeeId)
            || requesterEmployeeId == Guid.Empty)
        {
            return Forbid();
        }

        try
        {
            var result = await _service.CreateAsync(
                ticketId,
                requesterUserId,
                requesterEmployeeId,
                request);

            return StatusCode(
                StatusCodes.Status201Created,
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET: api/ticket-reopen-requests/tickets/{ticketId}
    [HttpGet("tickets/{ticketId:guid}")]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    public async Task<IActionResult> GetByTicketId(Guid ticketId)
    {
        Guid? requesterEmployeeId = null;

        if (User.IsInRole("Requester"))
        {
            var employeeIdClaim = User.FindFirst("employee_id")?.Value;

            if (!Guid.TryParse(employeeIdClaim, out var employeeId)
                || employeeId == Guid.Empty)
            {
                return Forbid();
            }

            requesterEmployeeId = employeeId;
        }

        try
        {
            var result = await _service.GetByTicketIdAsync(
                ticketId,
                requesterEmployeeId);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // PATCH: api/ticket-reopen-requests/{requestId}/review
    [HttpPatch("{requestId:guid}/review")]
    [Authorize(Roles = "Admin,ITStaff")]
    public async Task<IActionResult> Review(
        Guid requestId,
        [FromBody] ReviewTicketReopenRequest request)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var reviewerUserId)
            || reviewerUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _service.ReviewAsync(
                requestId,
                reviewerUserId,
                request);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET: api/ticket-reopen-requests
    [HttpGet]
    [Authorize(Roles = "Admin,ITStaff")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] TicketReopenRequestQueryParameters parameters)
    {
        var result = await _service.GetPagedAsync(parameters);

        return Ok(result);
    }
}

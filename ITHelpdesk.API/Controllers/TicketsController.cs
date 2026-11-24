
using ITHelpdesk.Application.DTOs.Pagination;
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITHelpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ITicketCommentService _ticketCommentService;
    private readonly ITicketAttachmentService _attachmentService;
    private readonly ITicketAssignmentService _assignmentService;

    public TicketsController(ITicketService ticketService, ITicketCommentService ticketCommentService, ITicketAttachmentService attachmentService, ITicketAssignmentService assignmentService)
    {
        _ticketService = ticketService;
        _ticketCommentService = ticketCommentService;
        _attachmentService = attachmentService;
        _assignmentService = assignmentService;
    }

    // GET: api/tickets
    [HttpGet]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    [ProducesResponseType(
        typeof(PagedResponse<TicketResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<TicketResponse>>> GetAll(
        [FromQuery] TicketQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        Guid? requesterId = null;

        // Requester chỉ được xem Ticket của chính mình
        if (User.IsInRole("Requester"))
        {
            var employeeIdClaim = User.FindFirst("employee_id")?.Value;

            if (!Guid.TryParse(employeeIdClaim, out var employeeId)
                || employeeId == Guid.Empty)
            {
                return Forbid();
            }

            requesterId = employeeId;
        }

        try
        {
            var tickets = await _ticketService.GetPagedTicketsAsync(
                parameters,
                requesterId,
                cancellationToken);

            return Ok(tickets);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/tickets/{id}
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TicketResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        Guid? requesterId = null;

        if (User.IsInRole("Requester"))
        {
            var employeeIdClaim = User.FindFirst("employee_id")?.Value;

            if (!Guid.TryParse(employeeIdClaim, out var employeeId)
                || employeeId == Guid.Empty)
            {
                return Forbid();
            }

            requesterId = employeeId;
        }

        var ticket = await _ticketService.GetTicketByIdAsync(
            id,
            requesterId,
            cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    // POST: api/tickets
    [HttpPost]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketRequest request,
        [FromQuery] Guid? requesterId,
        CancellationToken cancellationToken)
    {
        Guid resolvedRequesterId;

        // Admin và ITStaff được phép chọn Employee tạo Ticket thay mặt
        if (User.IsInRole("Admin") || User.IsInRole("ITStaff"))
        {
            if (!requesterId.HasValue || requesterId.Value == Guid.Empty)
            {
                return BadRequest(new
                {
                    message = "requesterId is required for Admin and ITStaff."
                });
            }

            resolvedRequesterId = requesterId.Value;
        }
        // Requester chỉ được tạo Ticket cho chính mình
        else if (User.IsInRole("Requester"))
        {
            var employeeIdClaim = User.FindFirst("employee_id")?.Value;

            if (!Guid.TryParse(employeeIdClaim, out resolvedRequesterId)
                || resolvedRequesterId == Guid.Empty)
            {
                return Forbid();
            }

            // Không cho phép Requester giả mạo Employee khác
            if (requesterId.HasValue &&
                requesterId.Value != resolvedRequesterId)
            {
                return Forbid();
            }
        }
        else
        {
            return Forbid();
        }

        var ticketId = await _ticketService.CreateTicketAsync(
            request,
            resolvedRequesterId,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = ticketId },
            new { id = ticketId });
    }

    // PUT: api/tickets/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ITStaff")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketResponse>> Update(
        Guid id,
        [FromBody] UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.UpdateTicketAsync(
            id,
            request,
            cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
    Guid id,
    CancellationToken cancellationToken)
    {
        var isDeleted = await _ticketService.DeleteTicketAsync(
            id,
            cancellationToken);

        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,ITStaff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
    Guid id,
    [FromBody] UpdateTicketStatusRequest request,
    CancellationToken cancellationToken)
    {
        try
        {
            var isUpdated = await _ticketService.UpdateTicketStatusAsync(
                id,
                request,
                "System",
                cancellationToken);

            if (!isUpdated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/tickets/{id}/history
    [HttpGet("{id:guid}/history")]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        Guid? requesterId = null;

        if (User.IsInRole("Requester"))
        {
            var employeeIdClaim = User.FindFirst("employee_id")?.Value;

            if (!Guid.TryParse(employeeIdClaim, out var employeeId)
                || employeeId == Guid.Empty)
            {
                return Forbid();
            }

            requesterId = employeeId;
        }

        try
        {
            var histories = await _ticketService.GetTicketHistoryAsync(
                id,
                requesterId,
                cancellationToken);

            return Ok(histories);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }


    [HttpPost("{id:guid}/comments")]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(
    Guid id,
    [FromBody] CreateTicketCommentRequest request,
    CancellationToken cancellationToken)
    {
        // Lấy Identity UserId của người đang đăng nhập.
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var createdByUserId)
            || createdByUserId == Guid.Empty)
        {
            return Forbid();
        }

        Guid? requesterId = null;

        // Nếu là Requester, lấy EmployeeId để kiểm tra quyền truy cập Ticket.
        if (User.IsInRole("Requester"))
        {
            var employeeIdClaim = User.FindFirst("employee_id")?.Value;

            if (!Guid.TryParse(employeeIdClaim, out var employeeId)
                || employeeId == Guid.Empty)
            {
                return Forbid();
            }

            requesterId = employeeId;
        }

        try
        {
            var commentId = await _ticketCommentService.AddCommentAsync(
                id,
                request,
                createdByUserId,
                requesterId,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new { id = commentId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}/comments")]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    [ProducesResponseType(
        typeof(IReadOnlyList<TicketCommentResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComments(
        Guid id,
        CancellationToken cancellationToken)
    {
        Guid? requesterId = null;

        if (User.IsInRole("Requester"))
        {
            var employeeIdClaim = User.FindFirst("employee_id")?.Value;

            if (!Guid.TryParse(employeeIdClaim, out var employeeId)
                || employeeId == Guid.Empty)
            {
                return Forbid();
            }

            requesterId = employeeId;
        }

        try
        {
            var comments = await _ticketCommentService.GetCommentsAsync(
                id,
                requesterId,
                cancellationToken);

            return Ok(comments);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/attachments")]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAttachment(
     Guid id,
     [FromForm(Name = "file")] IFormFile? file,
     CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Please select a file.");

        try
        {
            await using var stream = file.OpenReadStream();

            var attachmentId = await _attachmentService.UploadAsync(
                id,
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetAttachments),
                new { id },
                new { attachmentId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id:guid}/attachments")]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    public async Task<IActionResult> GetAttachments(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var attachments = await _attachmentService.GetByTicketIdAsync(
                id, cancellationToken);

            return Ok(attachments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    public async Task<IActionResult> DownloadAttachment(
        Guid id,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var stream = await _attachmentService.DownloadAsync(
                attachmentId, cancellationToken);

            if (stream == null)
                return NotFound("File not found on storage.");

            var attachment = (await _attachmentService.GetByTicketIdAsync(
                id, cancellationToken))
                .FirstOrDefault(a => a.Id == attachmentId);

            if (attachment == null)
            {
                await stream.DisposeAsync();
                return NotFound("Attachment does not belong to this ticket.");
            }

            return File(
                stream,
                attachment.ContentType,
                attachment.FileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Roles = "Admin,ITStaff")]
    public async Task<IActionResult> DeleteAttachment(
        Guid id,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Kiểm tra attachment có thuộc Ticket này không
            var attachments = await _attachmentService.GetByTicketIdAsync(
                id, cancellationToken);

            var attachment = attachments.FirstOrDefault(
                a => a.Id == attachmentId);

            if (attachment == null)
                return NotFound("Attachment not found in this ticket.");

            await _attachmentService.DeleteAsync(
                attachmentId, cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id:guid}/assignment")]
    [Authorize(Roles = "Admin,ITStaff")]
    public async Task<IActionResult> AssignTicket(
    Guid id,
    [FromBody] AssignTicketRequest request,
    CancellationToken cancellationToken)
    {
        try
        {
            await _assignmentService.AssignAsync(
                id,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:guid}/assignment")]
    [Authorize(Roles = "Admin,ITStaff")]
    public async Task<IActionResult> UnassignTicket(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _assignmentService.UnassignAsync(
                id,
                cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id:guid}/assignment")]
    [Authorize(Roles = "Admin,ITStaff,Requester")]
    public async Task<IActionResult> GetAssignment(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _assignmentService.GetAssignmentAsync(
                id,
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

}

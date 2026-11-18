
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Application.Services;
using Microsoft.AspNetCore.Mvc;
using ITHelpdesk.Application.Interfaces.Services;

namespace ITHelpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [ProducesResponseType(typeof(IReadOnlyList<TicketResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var tickets = await _ticketService.GetAllTicketsAsync(cancellationToken);

        return Ok(tickets);
    }

    // GET: api/tickets/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id, cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    // POST: api/tickets
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketRequest request,
        [FromQuery] Guid requesterId,
        CancellationToken cancellationToken)
    {
        var ticketId = await _ticketService.CreateTicketAsync(
            request,
            requesterId,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = ticketId },
            new { id = ticketId });
    }

    // PUT: api/tickets/{id}
    [HttpPut("{id:guid}")]
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
    [ProducesResponseType(typeof(IReadOnlyList<TicketHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var histories = await _ticketService.GetTicketHistoryAsync(
                id,
                cancellationToken);

            return Ok(histories);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }


    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(
        Guid id,
        [FromBody] CreateTicketCommentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var commentId = await _ticketCommentService.AddCommentAsync(
                id,
                request,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new { id = commentId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketCommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComments(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var comments = await _ticketCommentService.GetCommentsAsync(
                id,
                cancellationToken);

            return Ok(comments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/attachments")]
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

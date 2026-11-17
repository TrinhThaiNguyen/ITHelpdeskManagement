
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpdesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
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
}

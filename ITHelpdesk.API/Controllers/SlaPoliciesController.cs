using ITHelpdesk.Application.DTOs.SlaPolicies;
using ITHelpdesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpdesk.API.Controllers;

[ApiController]
[Route("api/sla-policies")]
[Authorize(Roles = "Admin")]
public class SlaPoliciesController : ControllerBase
{
    private readonly ISlaPolicyService _slaPolicyService;

    public SlaPoliciesController(
        ISlaPolicyService slaPolicyService)
    {
        _slaPolicyService = slaPolicyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _slaPolicyService.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _slaPolicyService.GetByIdAsync(
            id,
            cancellationToken);

        if (result == null)
        {
            return NotFound(new
            {
                message = "SLA Policy not found."
            });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSlaPolicyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _slaPolicyService.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSlaPolicyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _slaPolicyService.UpdateAsync(
                id,
                request,
                cancellationToken);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "SLA Policy not found."
                });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetActive(
        Guid id,
        [FromQuery] bool isActive,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _slaPolicyService.SetActiveAsync(
                id,
                isActive,
                cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    message = "SLA Policy not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
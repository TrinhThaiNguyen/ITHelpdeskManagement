using ITHelpdesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpdesk.API.Controllers;

[ApiController]
[Route("api/sla-monitoring")]
[Authorize(Roles = "Admin,ITStaff")]
public class SlaMonitoringController : ControllerBase
{
    private readonly ISlaMonitoringService _slaMonitoringService;

    public SlaMonitoringController(
        ISlaMonitoringService slaMonitoringService)
    {
        _slaMonitoringService = slaMonitoringService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMonitoring(
        CancellationToken cancellationToken)
    {
        var result = await _slaMonitoringService.GetMonitoringAsync(
            cancellationToken);

        return Ok(result);
    }
}
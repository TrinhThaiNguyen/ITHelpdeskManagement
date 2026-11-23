using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITHelpdesk.API.Controllers;

[Route("api/notifications")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private Guid CurrentUserId
    {
        get
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (CurrentUserId == Guid.Empty)
            return Unauthorized();

        var notifications =
            await _notificationService.GetMyNotificationsAsync(
                CurrentUserId,
                page,
                pageSize);

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (CurrentUserId == Guid.Empty)
            return Unauthorized();

        var result =
            await _notificationService.GetUnreadCountAsync(
                CurrentUserId);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        if (CurrentUserId == Guid.Empty)
            return Unauthorized();

        var success = await _notificationService.MarkAsReadAsync(
            id,
            CurrentUserId);

        if (!success)
            return NotFound(new
            {
                message = "Notification not found."
            });

        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (CurrentUserId == Guid.Empty)
            return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(
            CurrentUserId);

        return NoContent();
    }

}
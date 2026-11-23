using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ITHelpdesk.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(
            ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");

        if (Guid.TryParse(userId, out var id))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"user:{id}");
        }

        await base.OnConnectedAsync();
    }
}
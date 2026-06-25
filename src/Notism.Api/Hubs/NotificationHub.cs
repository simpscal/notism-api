using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using Notism.Api.Extensions;

namespace Notism.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public const string AdminsGroup = "admins";

    public async Task SubscribeToNotifications()
    {
        var userId = Context.User!.GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

        if (Context.User!.GetRole() == "admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
        }
    }
}
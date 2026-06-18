using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using Notism.Api.Extensions;

namespace Notism.Api.Hubs;

[Authorize]
public class PaymentHub : Hub
{
    public const string AdminsGroup = "admins";

    public async Task SubscribeToPaymentEvents()
    {
        var userId = Context.User!.GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());

        if (Context.User!.GetRole() == "admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
        }
    }
}
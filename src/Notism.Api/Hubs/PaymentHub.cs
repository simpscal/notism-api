using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using Notism.Api.Extensions;

namespace Notism.Api.Hubs;

[Authorize]
public class PaymentHub : Hub
{
    public async Task SubscribeToPaymentEvents()
    {
        var userId = Context.User!.GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
    }
}

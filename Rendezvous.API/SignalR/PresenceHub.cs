using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Rendezvous.API.Extensions;

namespace Rendezvous.API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker presenceTracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User ?? throw new HubException("Cannot get current user claim.");
        var isOnline = await presenceTracker
            .UserConnectedAsync(user.GetUsername(), Context.ConnectionId);
        if (isOnline)
        {
            await Clients.Others.SendAsync("UserIsOnline", user.GetUsername());
        }

        var currentUsers = await presenceTracker.GetOnlineUsersAsync();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.User ?? throw new HubException("Cannot get current user claim.");
        var isOffline = await presenceTracker
            .UserDisconnectedAsync(user.GetUsername(), Context.ConnectionId);
        if (isOffline)
        {
            await Clients.Others.SendAsync("UserIsOffline", user.GetUsername());
        }

        await base.OnDisconnectedAsync(exception);
    }
}

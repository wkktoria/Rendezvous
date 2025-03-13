using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Rendezvous.API.Extensions;

namespace Rendezvous.API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker presenceTracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;

        if (user == null)
        {
            throw new HubException("Cannot get current user claim.");
        }

        await presenceTracker.UserConnectedAsync(user.GetUsername(), Context.ConnectionId);
        await Clients.Others.SendAsync("UserIsOnline", user.GetUsername());

        var currentUsers = await presenceTracker.GetOnlineUsersAsync();
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.User;

        if (user == null)
        {
            throw new HubException("Cannot get current user claim.");
        }

        await presenceTracker.UserDisconnectedAsync(user.GetUsername(), Context.ConnectionId);
        await Clients.Others.SendAsync("UserIsOffline", user.GetUsername());

        var currentUsers = await presenceTracker.GetOnlineUsersAsync();
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

        await base.OnDisconnectedAsync(exception);
    }
}

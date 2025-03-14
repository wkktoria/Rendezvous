namespace Rendezvous.API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> OnlineUsers = [];

    public Task UserConnectedAsync(string username, string connectionId)
    {
        lock (OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId);
            }
            else
            {
                OnlineUsers.Add(username, [connectionId]);
            }
        }

        return Task.CompletedTask;
    }

    public Task UserDisconnectedAsync(string username, string connectionId)
    {
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(username))
            {
                return Task.CompletedTask;
            }
            else
            {
                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsersAsync()
    {
        string[] onlineUsers;
        lock (OnlineUsers)
        {
            onlineUsers = [.. OnlineUsers.OrderBy(p => p.Key).Select(p => p.Key)];
        }

        return Task.FromResult(onlineUsers);
    }

    public static Task<List<string>> GetConnectionsForUserAsync(string username)
    {
        List<string> connectionIds = [];

        if (OnlineUsers.TryGetValue(username, out var connections))
        {
            lock (connections)
            {
                connectionIds = [.. connections];
            }
        }

        return Task.FromResult(connectionIds);
    }
}

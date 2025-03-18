using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Extensions;
using Rendezvous.API.Interfaces;

namespace Rendezvous.API.SignalR;

public class MessageHub(IMessageRepository messageRepository,
    IUserRepository userRepository, IMapper mapper,
    IHubContext<PresenceHub> presenceHubContext) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"];

        if (Context.User == null || string.IsNullOrEmpty(otherUser))
        {
            throw new Exception("Cannot join group.");
        }

        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroupAsync(groupName);
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await messageRepository
            .GetMessageThreadAsync(Context.User.GetUsername(), otherUser!);
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroupAsync();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageAsync(CreateMessageDto createMessageDto)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Could not get user.");

        if (username.ToLower() == createMessageDto.RecipientUsername.ToLower())
        {
            throw new HubException("You cannot message yourself.");
        }

        var sender = await userRepository
            .GetUserByUsernameAsync(username);
        var recipient = await userRepository
            .GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (sender == null || recipient == null || sender.UserName == null || recipient.UserName == null)
        {
            throw new HubException("Cannot send message.");
        }

        var message = new Message
        {
            Sender = sender,
            SenderUsername = sender.UserName,
            Recipient = recipient,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };
        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await messageRepository.GetMessageGroupAsync(groupName);

        if (group != null && group.Connections.Any(c => c.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker
                .GetConnectionsForUserAsync(recipient.UserName);

            if (connections != null && connections.Count != 0)
            {
                await presenceHubContext.Clients.Clients(connections).SendAsync("NewMessageReceived", new
                {
                    username = sender.UserName,
                    knownAs = sender.KnownAs
                });
            }
        }

        await messageRepository.AddMessageAsync(message);

        if (await messageRepository.SaveAllAsync())
        {
            await Clients.Group(groupName)
                .SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
    }

    private async Task<Group> AddToGroupAsync(string groupName)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Cannot get username.");
        var group = await messageRepository.GetMessageGroupAsync(groupName);
        var connection = new Connection
        {
            ConnectionId = Context.ConnectionId,
            Username = username
        };

        if (group == null)
        {
            group = new Group
            {
                Name = groupName
            };
            messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (await messageRepository.SaveAllAsync())
        {
            return group;
        }

        throw new HubException("Failed to join group.");
    }

    private async Task<Group> RemoveFromMessageGroupAsync()
    {
        var connectionId = Context.ConnectionId;
        var group = await messageRepository.GetGroupForConnectionAsync(connectionId);
        var connection = group?.Connections.FirstOrDefault(c => c.ConnectionId == connectionId);

        if (connection != null && group != null)
        {
            messageRepository.RemoveConnection(connection);
            if (await messageRepository.SaveAllAsync())
            {
                return group;
            }
        }

        throw new HubException("Failed to remove from group.");
    }

    private static string GetGroupName(string caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}

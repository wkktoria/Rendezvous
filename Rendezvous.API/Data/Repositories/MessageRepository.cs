using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Helpers;
using Rendezvous.API.Interfaces.Repositories;

namespace Rendezvous.API.Data.Repositories;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddGroup(Group group)
    {
        context.Groups.Add(group);
    }

    public async Task AddMessageAsync(Message message)
    {
        await context.Messages.AddAsync(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Connection?> GetConnectionAsync(string connectionId)
    {
        return await context.Connections.FindAsync(connectionId);
    }

    public async Task<Group?> GetGroupForConnectionAsync(string connectionId)
    {
        return await context.Groups
            .Include(g => g.Connections)
            .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }

    public async Task<Message?> GetMessageAsync(int id)
    {
        return await context.Messages.FindAsync(id);
    }

    public async Task<Group?> GetMessageGroupAsync(string groupName)
    {
        return await context.Groups
            .Include(g => g.Connections)
            .FirstOrDefaultAsync(g => g.Name == groupName);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUserAsync(
        MessageParams messageParams)
    {
        var query = context.Messages
            .OrderByDescending(m => m.MessageSent)
            .AsQueryable();
        query = messageParams.Container switch
        {
            "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.Username && !m.RecipientDeleted),
            "Outbox" => query.Where(m => m.Sender.UserName == messageParams.Username && !m.SenderDeleted),
            _ => query.Where(m => m.Recipient.UserName == messageParams.Username && m.DateRead == null && !m.RecipientDeleted)
        };

        var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

        return await PagedList<MessageDto>
            .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(
        string currentUsername, string recipientUsername)
    {
        var messages = await context.Messages
            .Where(m => m.RecipientUsername == currentUsername && !m.RecipientDeleted && m.SenderUsername == recipientUsername
                || m.SenderUsername == currentUsername && !m.SenderDeleted && m.RecipientUsername == recipientUsername)
            .OrderBy(m => m.MessageSent)
            .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        var unreadMessages = messages
            .Where(m => m.DateRead == null && m.RecipientUsername == currentUsername)
            .ToList();

        if (unreadMessages.Count != 0)
        {
            unreadMessages.ForEach(m => m.DateRead = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }

        return messages;
    }

    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }
}

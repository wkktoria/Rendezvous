using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Helpers;
using Rendezvous.API.Interfaces;

namespace Rendezvous.API.Data.Repositories;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public async Task AddMessageAsync(Message message)
    {
        await context.Messages.AddAsync(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessageAsync(int id)
    {
        return await context.Messages.FindAsync(id);
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
            .Include(m => m.Sender).ThenInclude(u => u.Photos)
            .Include(m => m.Recipient).ThenInclude(u => u.Photos)
            .Where(m => m.RecipientUsername == currentUsername && !m.RecipientDeleted && m.SenderUsername == recipientUsername
                || m.SenderUsername == currentUsername && !m.SenderDeleted && m.RecipientUsername == recipientUsername)
            .OrderBy(m => m.MessageSent)
            .ToListAsync();

        var unreadMessages = messages
            .Where(m => m.DateRead == null && m.RecipientUsername == currentUsername)
            .ToList();

        if (unreadMessages.Count != 0)
        {
            unreadMessages.ForEach(m => m.DateRead = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }

        return mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}

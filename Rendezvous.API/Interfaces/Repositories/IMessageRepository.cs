using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Helpers;

namespace Rendezvous.API.Interfaces.Repositories;

public interface IMessageRepository
{
    Task AddMessageAsync(Message message);

    void DeleteMessage(Message message);

    Task<Message?> GetMessageAsync(int id);

    Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams);

    Task<IEnumerable<MessageDto>> GetMessageThreadAsync(
        string currentUsername, string recipientUsername);

    void AddGroup(Group group);

    void RemoveConnection(Connection connection);

    Task<Connection?> GetConnectionAsync(string connectionId);

    Task<Group?> GetMessageGroupAsync(string groupName);

    Task<Group?> GetGroupForConnectionAsync(string connectionId);
}

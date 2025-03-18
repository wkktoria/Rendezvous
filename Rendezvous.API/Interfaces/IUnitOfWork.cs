using Rendezvous.API.Interfaces.Repositories;

namespace Rendezvous.API.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }

    IMessageRepository MessageRepository { get; }

    ILikesRepository LikesRepository { get; }

    Task<bool> CompleteAsync();

    bool HasChanges();
}

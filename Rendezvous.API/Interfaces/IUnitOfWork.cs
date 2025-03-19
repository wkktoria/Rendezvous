using Rendezvous.API.Interfaces.Repositories;

namespace Rendezvous.API.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }

    IMessageRepository MessageRepository { get; }

    ILikesRepository LikesRepository { get; }

    IPhotoRepository PhotoRepository { get; }

    Task<bool> CompleteAsync();

    bool HasChanges();
}

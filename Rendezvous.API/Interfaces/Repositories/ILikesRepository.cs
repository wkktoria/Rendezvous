using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Helpers;

namespace Rendezvous.API.Interfaces.Repositories;

public interface ILikesRepository
{
    Task<UserLike?> GetUserLikeAsync(int sourceUserId, int targetUserId);

    Task<PagedList<MemberDto>> GetUserLikesAsync(LikesParams likesParams);

    Task<IEnumerable<int>> GetCurrentUserLikeIdsAsync(int currentUserId);

    void DeleteLike(UserLike like);

    Task AddLikeAsync(UserLike like);

    Task<bool> SaveChangesAsync();
}

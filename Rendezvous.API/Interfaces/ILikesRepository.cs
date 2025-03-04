using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;

namespace Rendezvous.API.Interfaces;

public interface ILikesRepository
{
    Task<UserLike?> GetUserLikeAsync(int sourceUserId, int targetUserId);

    Task<IEnumerable<MemberDto>> GetUserLikesAsync(string predicate, int userId);

    Task<IEnumerable<int>> GetCurrentUserLikeIdsAsync(int currentUserId);

    void DeleteLike(UserLike like);

    Task AddLikeAsync(UserLike like);

    Task<bool> SaveChangesAsync();
}

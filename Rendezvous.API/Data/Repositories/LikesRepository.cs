using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Interfaces;

namespace Rendezvous.API.Data.Repositories;

public class LikesRepository(DataContext context, IMapper mapper) : ILikesRepository
{
    public async Task AddLikeAsync(UserLike like)
    {
        await context.Likes.AddAsync(like);
    }

    public void DeleteLike(UserLike like)
    {
        context.Likes.Remove(like);
    }

    public async Task<IEnumerable<int>> GetCurrentUserLikeIdsAsync(int currentUserId)
    {
        return await context.Likes
            .Where(ul => ul.SourceUserId == currentUserId)
            .Select(ul => ul.TargetUserId)
            .ToListAsync();
    }

    public async Task<UserLike?> GetUserLikeAsync(int sourceUserId, int targetUserId)
    {
        return await context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<IEnumerable<MemberDto>> GetUserLikesAsync(string predicate, int userId)
    {
        var likes = context.Likes.AsQueryable();

        switch (predicate)
        {
            case "liked":
                return await likes.Where(ul => ul.SourceUserId == userId)
                    .Select(ul => ul.TargetUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
            case "likedBy":
                return await likes.Where(ul => ul.TargetUserId == userId)
                    .Select(ul => ul.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
            default:
                var likeIds = await GetCurrentUserLikeIdsAsync(userId);
                return await likes
                    .Where(ul => ul.TargetUserId == userId && likeIds.Contains(ul.SourceUserId))
                    .Select(ul => ul.SourceUser).ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}

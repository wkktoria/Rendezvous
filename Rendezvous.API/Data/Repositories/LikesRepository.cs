using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Helpers;
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

    public async Task<PagedList<MemberDto>> GetUserLikesAsync(LikesParams likesParams)
    {
        var likes = context.Likes.AsQueryable();
        IQueryable<MemberDto> query;

        switch (likesParams.Predicate)
        {
            case "liked":
                query = likes.Where(ul => ul.SourceUserId == likesParams.UserId)
                    .Select(ul => ul.TargetUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
            case "likedBy":
                query = likes.Where(ul => ul.TargetUserId == likesParams.UserId)
                    .Select(ul => ul.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
            default:
                var likeIds = await GetCurrentUserLikeIdsAsync(likesParams.UserId);
                query = likes
                    .Where(ul => ul.TargetUserId == likesParams.UserId && likeIds.Contains(ul.SourceUserId))
                    .Select(ul => ul.SourceUser).ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
        }

        return await PagedList<MemberDto>
            .CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}

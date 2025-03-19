using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Helpers;
using Rendezvous.API.Interfaces.Repositories;

namespace Rendezvous.API.Data.Repositories;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    public async Task<MemberDto?> GetMemberAsync(string username, bool isCurrentUser)
    {
        var query = context.Users
            .Where(u => u.UserName == username)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .AsQueryable();

        if (isCurrentUser)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = context.Users.AsQueryable();
        query = query.Where(user => user.UserName != userParams.CurrentUsername);

        if (userParams.Gender != null)
        {
            query = query.Where(user => user.Gender == userParams.Gender);
        }

        var minDateOfBirth = DateOnly
            .FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDateOfBirth = DateOnly
            .FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        query = query.Where(user =>
            user.DateOfBirth >= minDateOfBirth && user.DateOfBirth <= maxDateOfBirth);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(user => user.Created),
            _ => query.OrderByDescending(user => user.LastActive)
        };

        return await PagedList<MemberDto>
            .CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider),
                userParams.PageNumber, userParams.PageSize);
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetUserByPhotoIdAsync(int photoId)
    {
        return await context.Users
            .Include(u => u.Photos)
            .IgnoreQueryFilters()
            .Where(u => u.Photos.Any(p => p.Id == photoId))
            .FirstOrDefaultAsync();
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return await context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.UserName == username.ToLower());
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await context.Users.Include(u => u.Photos).ToListAsync();
    }

    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
}

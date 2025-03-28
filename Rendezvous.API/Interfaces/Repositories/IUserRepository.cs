using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Helpers;

namespace Rendezvous.API.Interfaces.Repositories;

public interface IUserRepository
{
    void Update(AppUser user);

    Task<IEnumerable<AppUser>> GetUsersAsync();

    Task<AppUser?> GetUserByIdAsync(int id);

    Task<AppUser?> GetUserByUsernameAsync(string username);

    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);

    Task<MemberDto?> GetMemberAsync(string username, bool isCurrentUser);

    Task<AppUser?> GetUserByPhotoIdAsync(int photoId);
}

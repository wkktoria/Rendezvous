using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;

namespace Rendezvous.API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);

    Task<bool> SaveAllAsync();

    Task<IEnumerable<AppUser>> GetUsersAsync();

    Task<AppUser?> GetUserByIdAsync(int id);

    Task<AppUser?> GetUserByUsernameAsync(string username);

    Task<IEnumerable<MemberDto>> GetMembersAsync();

    Task<MemberDto?> GetMemberAsync(string username);
}

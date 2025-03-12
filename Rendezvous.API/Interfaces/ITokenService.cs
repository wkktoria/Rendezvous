using Rendezvous.API.Entities;

namespace Rendezvous.API.Interfaces;

public interface ITokenService
{
    Task<string> CreateTokenAsync(AppUser user);
}

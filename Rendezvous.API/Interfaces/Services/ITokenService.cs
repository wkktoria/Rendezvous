using Rendezvous.API.Entities;

namespace Rendezvous.API.Interfaces.Services;

public interface ITokenService
{
    Task<string> CreateTokenAsync(AppUser user);
}

using Rendezvous.API.Entities;

namespace Rendezvous.API.Interfaces;

public interface ITokenService
{
    string CreateToken(AppUser user);
}

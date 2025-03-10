using Microsoft.AspNetCore.Identity;

namespace Rendezvous.API.Entities;

public class AppRole : IdentityRole<int>
{
    public ICollection<AppUserRole> UserRoles { get; set; } = [];
}

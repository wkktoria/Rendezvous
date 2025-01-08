using Microsoft.EntityFrameworkCore;
using Rendezvous.API.Entities;

namespace Rendezvous.API.Data;

public class DataContext(DbContextOptions options) : DbContext(options)
{
    public required DbSet<AppUser> Users { get; set; }
}

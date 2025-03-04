using Microsoft.EntityFrameworkCore;
using Rendezvous.API.Data;
using Rendezvous.API.Data.Repositories;
using Rendezvous.API.Helpers;
using Rendezvous.API.Interfaces;
using Rendezvous.API.Services;

namespace Rendezvous.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddControllers();
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
        services.AddCors();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<LogUserActivity>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.Configure<CloudinarySettings>(config.GetSection(nameof(CloudinarySettings)));

        return services;
    }
}

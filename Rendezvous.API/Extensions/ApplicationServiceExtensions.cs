using Microsoft.EntityFrameworkCore;
using Rendezvous.API.Data;
using Rendezvous.API.Data.Repositories;
using Rendezvous.API.Helpers;
using Rendezvous.API.Interfaces;
using Rendezvous.API.Interfaces.Repositories;
using Rendezvous.API.Interfaces.Services;
using Rendezvous.API.Services;
using Rendezvous.API.SignalR;

namespace Rendezvous.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddControllers();

        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        });

        services.AddCors();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILikesRepository, LikesRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<LogUserActivity>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<PresenceTracker>();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddSignalR();
        services.Configure<CloudinarySettings>(config.GetSection(nameof(CloudinarySettings)));

        return services;
    }
}

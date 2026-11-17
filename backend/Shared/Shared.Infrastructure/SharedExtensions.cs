using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure;

public static class SharedExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<SharedDbContext>(configuration, "shared");
        services.AddHostedService<SharedInitializer>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SharedDbContext>());

        services.AddSharedImages();
        services.AddSharedPositions();

        return services;
    }
}

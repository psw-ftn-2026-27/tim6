using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application;
using Shared.Application.ExplorerPositioning;
using Shared.Application.ImageReading;
using Shared.Application.Images;
using Shared.Application.ImageUploading;
using Shared.Application.Positions;
using Shared.Infrastructure.Images;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Images;
using Shared.Infrastructure.Persistence.Positions;

namespace Shared.Infrastructure;

public static class SharedExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<SharedDbContext>(configuration, "shared");
        services.AddHostedService<SharedInitializer>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SharedDbContext>());

        AddImages(services);
        AddPositions(services);

        return services;
    }

    private static void AddImages(IServiceCollection services)
    {
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IImageStorage, ImageStorage>();
        services.AddScoped<IImageProcessor, ImageProcessor>();

        services.AddScoped<ImageUploadingService>();
        services.AddScoped<ImageReadingQueries>();
    }

    private static void AddPositions(IServiceCollection services)
    {
        services.AddScoped<IExplorerPositionRepository, ExplorerPositionRepository>();
        services.AddScoped<IExplorerPositionReadRepository, ExplorerPositionReadRepository>();

        services.AddScoped<ExplorerPositioningService>();
        services.AddScoped<ExplorerPositioningQueries>();
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application;
using Shared.Application.ImageReading;
using Shared.Application.Images;
using Shared.Application.ImageUploading;
using Shared.Infrastructure.Images;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Images;

namespace Shared.Infrastructure;

public static class SharedImagesExtensions
{
    public static IServiceCollection AddSharedImages(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<SharedDbContext>(configuration, "shared");
        services.AddHostedService<SharedInitializer>();

        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SharedDbContext>());
        services.AddScoped<IImageStorage, ImageStorage>();
        services.AddScoped<IImageProcessor, ImageProcessor>();

        services.AddScoped<ImageUploadingService>();
        services.AddScoped<ImageReadingQueries>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Shared.Application.ImageReading;
using Shared.Application.Images;
using Shared.Application.ImageUploading;
using Shared.Infrastructure.Images;
using Shared.Infrastructure.Persistence.Images;

namespace Shared.Infrastructure;

internal static class SharedImagesExtensions
{
    public static IServiceCollection AddSharedImages(this IServiceCollection services)
    {
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IImageStorage, ImageStorage>();
        services.AddScoped<IImageProcessor, ImageProcessor>();

        services.AddScoped<ImageUploadingService>();
        services.AddScoped<ImageReadingQueries>();

        return services;
    }
}

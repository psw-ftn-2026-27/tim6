using Microsoft.Extensions.DependencyInjection;

namespace Shared.Api;

public static class SharedImagesApiExtensions
{
    public static IMvcBuilder AddSharedImagesControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(SharedImagesApiExtensions).Assembly);
    }
}

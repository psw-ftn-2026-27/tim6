using Microsoft.Extensions.DependencyInjection;

namespace Shared.Api;

public static class SharedApiExtensions
{
    public static IMvcBuilder AddSharedControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(SharedApiExtensions).Assembly);
    }
}

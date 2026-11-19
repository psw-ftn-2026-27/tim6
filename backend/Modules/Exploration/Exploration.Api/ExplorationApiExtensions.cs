using Microsoft.Extensions.DependencyInjection;
using Exploration.Application.TourConversationMessaging;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace Exploration.Api;

public static class ExplorationApiExtensions
{
    public static IMvcBuilder AddExplorationControllers(this IMvcBuilder mvc)
    {
        mvc.Services.AddScoped<IConversationNotifier, SignalRConversationNotifier>();
        return mvc.AddApplicationPart(typeof(ExplorationApiExtensions).Assembly);
    }

    public static IEndpointRouteBuilder MapExplorationConversationHub(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<TourConversationHub>("/hubs/exploration/conversations");
        return endpoints;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Social.Application.DirectConversationMessaging;

namespace Social.Api;

public static class SocialApiExtensions
{
    public static IMvcBuilder AddSocialControllers(this IMvcBuilder mvc)
    {
        mvc.Services.AddScoped<IConversationNotifier, SignalRConversationNotifier>();
        return mvc.AddApplicationPart(typeof(SocialApiExtensions).Assembly);
    }

    public static IEndpointRouteBuilder MapSocialConversationHub(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<DirectConversationHub>("/hubs/social/conversations");
        return endpoints;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Shared.Application.ExplorerPositioning;
using Shared.Application.Positions;
using Shared.Infrastructure.Persistence.Positions;

namespace Shared.Infrastructure;

internal static class SharedPositionsExtensions
{
    public static IServiceCollection AddSharedPositions(this IServiceCollection services)
    {
        services.AddScoped<IExplorerPositionRepository, ExplorerPositionRepository>();
        services.AddScoped<IExplorerPositionReadRepository, ExplorerPositionReadRepository>();

        services.AddScoped<ExplorerPositioningService>();
        services.AddScoped<ExplorerPositioningQueries>();

        return services;
    }
}

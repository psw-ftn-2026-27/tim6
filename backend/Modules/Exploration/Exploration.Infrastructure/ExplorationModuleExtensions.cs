using Exploration.Application;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.TourPreferenceEditing;
using Exploration.Application.TourPreferences;
using Exploration.Application.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Infrastructure.Persistence.TourPreferences;
using Exploration.Infrastructure.Persistence.Tours;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Exploration.Application.Conversations;
using Exploration.Application.TourConversationMessaging;
using Exploration.Infrastructure.Persistence.Conversations;

namespace Exploration.Infrastructure;

public static class ExplorationModuleExtensions
{
    public static IServiceCollection AddExplorationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<ExplorationDbContext>(configuration, "exploration");
        services.AddHostedService<ExplorationModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<ExplorationMapperProfile>());

        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ExplorationDbContext>());
        services.AddScoped<ITourReadRepository, TourReadRepository>();
        services.AddScoped<ITourPreferenceRepository, TourPreferenceRepository>();
        services.AddScoped<ITourPreferenceReadRepository, TourPreferenceReadRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IConversationReadRepository, ConversationReadRepository>();

        services.AddScoped<TourAuthoringService>();
        services.AddScoped<TourAuthoringQueries>();
        services.AddScoped<KeyPointAuthoringService>();
        services.AddScoped<KeyPointAuthoringQueries>();
        services.AddScoped<TourBrowsingQueries>();
        services.AddScoped<TourPreferenceEditingService>();
        services.AddScoped<TourPreferenceEditingQueries>();
        services.AddScoped<TourConversationMessagingService>();
        services.AddScoped<TourConversationMessagingQueries>();

        return services;
    }
}

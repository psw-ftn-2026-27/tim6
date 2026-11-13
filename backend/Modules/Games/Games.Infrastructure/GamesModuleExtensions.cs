using Games.Application;
using Games.Application.QuizAuthoring;
using Games.Application.Quizzes;
using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallengePublishing;
using Games.Application.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Games.Infrastructure;

public static class GamesModuleExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GamesDbContext>(configuration, "games");
        services.AddHostedService<GamesModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<GamesMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GamesDbContext>());
        services.AddScoped<ISocialChallengeRepository, SocialChallengeRepository>();
        services.AddScoped<ISocialChallengeReadRepository, SocialChallengeReadRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizReadRepository, QuizReadRepository>();

        services.AddScoped<SocialChallengeAuthoringService>();
        services.AddScoped<SocialChallengeAuthoringQueries>();
        services.AddScoped<SocialChallengePublishingService>();
        services.AddScoped<QuizAuthoringService>();
        services.AddScoped<QuizAuthoringQueries>();

        return services;
    }
}

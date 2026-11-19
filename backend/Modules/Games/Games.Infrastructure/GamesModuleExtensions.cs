using Games.Application;
using Games.Application.ChallengeProgresses;
using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallengePublishing;
using Games.Application.LocationChallengeSolving;
using Games.Application.LocationChallenges;
using Games.Application.QuizAttempts;
using Games.Application.QuizAuthoring;
using Games.Application.QuizPublishing;
using Games.Application.QuizSolving;
using Games.Application.Quizzes;
using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallengePublishing;
using Games.Application.SocialChallenges;
using Games.Domain.LocationChallengeAttempts;
using Games.Infrastructure.Persistence;
using Games.Infrastructure.Persistence.ChallengeProgresses;
using Games.Infrastructure.Persistence.LocationChallengeAttempts;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.QuizAttempts;
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
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<IQuizAttemptReadRepository, QuizAttemptReadRepository>();
        services.AddScoped<IChallengeProgressRepository, ChallengeProgressRepository>();
        services.AddScoped<ILocationChallengeRepository, LocationChallengeRepository>();
        services.AddScoped<ILocationChallengeReadRepository, LocationChallengeReadRepository>();
        services.AddScoped<ILocationChallengeAttemptRepository, LocationChallengeAttemptRepository>();
        services.AddScoped<ILocationChallengeAttemptReadRepository, LocationChallengeAttemptReadRepository>();

        services.AddScoped<SocialChallengeAuthoringService>();
        services.AddScoped<SocialChallengeAuthoringQueries>();
        services.AddScoped<SocialChallengePublishingService>();
        services.AddScoped<QuizAuthoringService>();
        services.AddScoped<QuizAuthoringQueries>();
        services.AddScoped<QuizPublishingService>();
        services.AddScoped<QuizSolvingService>();
        services.AddScoped<QuizSolvingQueries>();
        services.AddScoped<LocationChallengeAuthoringService>();
        services.AddScoped<LocationChallengeAuthoringQueries>();
        services.AddScoped<LocationChallengePublishingService>();
        services.AddScoped<LocationChallengeSolver>();
        services.AddScoped<LocationChallengeSolvingService>();
        services.AddScoped<LocationChallengeSolvingQueries>();

        return services;
    }
}

using Games.Application;
using Games.Domain.ChallengeProgresses;
using Games.Domain.LocationChallenges;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence.ChallengeProgresses;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.QuizAttempts;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence;

internal sealed class GamesDbContext : DbContext, IUnitOfWork
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }

    public DbSet<SocialChallenge> SocialChallenges => Set<SocialChallenge>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<ChallengeProgress> ChallengeProgresses => Set<ChallengeProgress>();
    public DbSet<LocationChallenge> LocationChallenges => Set<LocationChallenge>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("games");
        builder.ConfigureSocialChallenges();
        builder.ConfigureQuizzes();
        builder.ConfigureQuizAttempts();
        builder.ConfigureChallengeProgresses();
        builder.ConfigureLocationChallenges();
    }
}

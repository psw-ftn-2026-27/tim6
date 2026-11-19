using Games.Domain.QuizAttempts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.QuizAttempts;

internal static class QuizAttemptConfiguration
{
    public static void ConfigureQuizAttempts(this ModelBuilder modelBuilder)
    {
        ConfigureQuizAttempt(modelBuilder.Entity<QuizAttempt>());
    }

    private static void ConfigureQuizAttempt(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("QuizAttempts");

        builder.OwnsMany(attempt => attempt.Answers, answers =>
        {
            answers.ToJson();
        });
    }
}

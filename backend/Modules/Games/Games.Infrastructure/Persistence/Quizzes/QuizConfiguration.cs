using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.Quizzes;

internal static class QuizConfiguration
{
    public static void ConfigureQuizzes(this ModelBuilder modelBuilder)
    {
        ConfigureQuiz(modelBuilder.Entity<Quiz>());
    }

    private static void ConfigureQuiz(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes");
        builder.Property(quiz => quiz.Name).HasMaxLength(200);
        builder.Property(quiz => quiz.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(quiz => quiz.Location);

        builder.OwnsMany(quiz => quiz.Questions, questions =>
        {
            questions.ToJson();
            questions.OwnsMany(question => question.Options);
        });
    }
}

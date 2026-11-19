using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal static class SocialChallengeConfiguration
{
    // The run's moments are in the city's local time, so they are stored without a time zone.
    private const string LocalTimeColumnType = "timestamp without time zone";

    public static void ConfigureSocialChallenges(this ModelBuilder modelBuilder)
    {
        ConfigureSocialChallenge(modelBuilder.Entity<SocialChallenge>());
        ConfigureSocialChallengeRun(modelBuilder.Entity<SocialChallengeRun>());
        ConfigureRunParticipant(modelBuilder.Entity<RunParticipant>());
    }

    private static void ConfigureSocialChallenge(EntityTypeBuilder<SocialChallenge> builder)
    {
        builder.ToTable("SocialChallenges");
        builder.Property(challenge => challenge.Name).HasMaxLength(200);
        builder.Property(challenge => challenge.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(challenge => challenge.TimeWindow);
        builder.OwnsOne(challenge => challenge.Location);

        builder.HasOne(challenge => challenge.ActiveRun)
            .WithOne()
            .HasForeignKey<SocialChallengeRun>("SocialChallengeId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(challenge => challenge.ActiveRun).AutoInclude();
    }

    private static void ConfigureSocialChallengeRun(EntityTypeBuilder<SocialChallengeRun> builder)
    {
        builder.ToTable("SocialChallengeRuns");
        builder.Property(run => run.Id).ValueGeneratedNever();
        builder.Property(run => run.StartedAt).HasColumnType(LocalTimeColumnType);

        builder.HasMany(run => run.Participants)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(run => run.Participants).AutoInclude();
    }

    private static void ConfigureRunParticipant(EntityTypeBuilder<RunParticipant> builder)
    {
        builder.ToTable("RunParticipants");
        builder.Property(participant => participant.Id).ValueGeneratedNever();
        builder.Property(participant => participant.LastCheckedAt).HasColumnType(LocalTimeColumnType);
    }
}

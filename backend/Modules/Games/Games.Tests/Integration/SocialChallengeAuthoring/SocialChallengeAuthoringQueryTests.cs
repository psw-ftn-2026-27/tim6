using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Challenges;
using Games.Application.SocialChallengeAuthoring;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeAuthoring;

public class SocialChallengeAuthoringQueryTests : BaseIntegrationTest
{
    public SocialChallengeAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_every_social_challenge_with_its_data_newest_first()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<SocialChallengeDto>>("/api/games/social-challenges", JsonOptions);

        var expected = SocialChallengeSeed.All
            .OrderByDescending(challenge => challenge.CreatedAt.Ticks / TimeSpan.TicksPerMicrosecond)
            .ThenBy(challenge => challenge.Id)
            .Select(challenge => new SocialChallengeDto(
            challenge.Id,
            challenge.Name,
            challenge.Status,
            new DailyTimeWindowDto(challenge.TimeWindow.Start, challenge.TimeWindow.End),
            challenge.RequiredPeople,
            challenge.RangeInMeters,
            challenge.Xp,
            challenge.ExitMessage,
            challenge.Location is null ? null : new LocationDto(challenge.Location.Latitude, challenge.Location.Longitude)));
        challenges.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Administrator_sees_the_location_of_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<SocialChallengeDto>>("/api/games/social-challenges", JsonOptions);

        var seeded = SocialChallengeSeed.LocatedDraftFortressSunset;
        challenges!.Single(challenge => challenge.Id == seeded.Id).Location
            .Should().Be(new LocationDto(seeded.Location!.Latitude, seeded.Location.Longitude));
    }
}

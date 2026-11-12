using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeAuthoring;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeAuthoring;

public class SocialChallengeAuthoringQueryTests : BaseIntegrationTest
{
    public SocialChallengeAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_every_social_challenge_with_its_data()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<SocialChallengeDto>>("/api/games/social-challenges", JsonOptions);

        var expected = SocialChallengeSeed.All.Select(challenge => new SocialChallengeDto(
            challenge.Id,
            challenge.Name,
            challenge.Status,
            new DailyTimeWindowDto(challenge.TimeWindow.Start, challenge.TimeWindow.End),
            challenge.RequiredPeople,
            challenge.RangeInMeters,
            challenge.Xp,
            challenge.ExitMessage));
        challenges.Should().BeEquivalentTo(expected);
    }
}

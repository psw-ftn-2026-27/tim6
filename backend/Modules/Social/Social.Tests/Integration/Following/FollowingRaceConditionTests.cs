using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Tests;
using Social.Domain.Following;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.Following;

/// <summary>
/// Two requests can pass the application checks at the same time. The unique indexes are the last
/// line of defence, and SocialDbContext turns exactly those violations into a client error.
/// A rival transaction is held open here so the request under test reaches the database, blocks on
/// the unique index, and only fails once the rival commits. This is the one place a test writes to
/// the database directly: there is no other way to hold the index.
/// </summary>
public class FollowingRaceConditionTests : BaseIntegrationTest
{
    private static readonly TimeSpan BlockTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(20);

    public FollowingRaceConditionTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task A_request_racing_another_pending_request_for_the_same_pair_is_a_bad_request()
    {
        using var rival = Factory.CreateContext<SocialDbContext>();
        await using var transaction = await rival.Database.BeginTransactionAsync();
        rival.FollowRequests.Add(new FollowRequest(WellKnownUsers.Administrator, WellKnownUsers.Explorer));
        await rival.SaveChangesAsync();

        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var racing = client.PostAsync($"/api/social/users/{WellKnownUsers.Explorer}/follow-requests", null);
        await AssertBlockedOnIndexAsync(racing);
        await transaction.CommitAsync();

        var response = await racing;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<SocialDbContext>();
        context.FollowRequests.Count(request =>
            request.SenderId == WellKnownUsers.Administrator && request.RecipientId == WellKnownUsers.Explorer)
            .Should().Be(1);
    }

    [Fact]
    public async Task Accepting_a_request_while_the_same_follow_is_being_created_is_a_bad_request()
    {
        using var rival = Factory.CreateContext<SocialDbContext>();
        await using var transaction = await rival.Database.BeginTransactionAsync();
        rival.Follows.Add(new Follow(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer));
        await rival.SaveChangesAsync();

        var requestId = FollowSeed.SecondExplorerPendingRequestToExplorer.Id;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var racing = client.PostAsync($"/api/social/follow-requests/{requestId}/accept", null);
        await AssertBlockedOnIndexAsync(racing);
        await transaction.CommitAsync();

        var response = await racing;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<SocialDbContext>();
        context.FollowRequests.Single(request => request.Id == requestId).Status
            .Should().Be(FollowRequestStatus.Pending, "the failed accept must be rolled back as a whole");
        context.Follows.Count(follow =>
            follow.FollowerId == WellKnownUsers.SecondExplorer && follow.FollowedUserId == WellKnownUsers.Explorer)
            .Should().Be(1);
    }

    [Fact]
    public async Task Other_unique_violations_are_not_reported_as_follow_conflicts()
    {
        var existing = new Follow(WellKnownUsers.Administrator, WellKnownUsers.Explorer);
        using (var first = Factory.CreateContext<SocialDbContext>())
        {
            first.Follows.Add(existing);
            await first.SaveChangesAsync();
        }

        using var second = Factory.CreateContext<SocialDbContext>();
        var sameKeyDifferentPair = new Follow(WellKnownUsers.Explorer, WellKnownUsers.Administrator);
        second.Follows.Add(sameKeyDifferentPair);
        second.Entry(sameKeyDifferentPair).Property(follow => follow.Id).CurrentValue = existing.Id;

        var act = () => second.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>("a primary key violation is not one of the expected follow indexes");
    }

    /// <summary>
    /// Waits until PostgreSQL itself reports a backend of this database blocked on a transaction lock, which is
    /// what an insert into a unique index waiting for the rival's uncommitted row looks like. This replaces a
    /// fixed sleep: the test proceeds the moment the request is really blocked, and fails clearly if it never
    /// is, or if the request finishes without ever blocking.
    /// </summary>
    private async Task AssertBlockedOnIndexAsync(Task<HttpResponseMessage> request)
    {
        using var observer = Factory.CreateContext<SocialDbContext>();
        var connection = observer.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*) FROM pg_stat_activity
            WHERE datname = current_database() AND wait_event_type = 'Lock' AND wait_event = 'transactionid'
            """;

        var deadline = DateTime.UtcNow + BlockTimeout;
        while (true)
        {
            request.IsCompleted.Should().BeFalse(
                "the request should be waiting on the unique index held by the rival transaction");

            if ((long)(await command.ExecuteScalarAsync())! > 0)
            {
                return;
            }

            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException(
                    $"The request was not blocked on the rival transaction within {BlockTimeout.TotalSeconds} seconds.");
            }

            await Task.Delay(PollInterval);
        }
    }
}

using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.FollowRelationships;
using Xunit;

namespace Social.Tests.Unit.FollowRelationships;

public class FollowRelationshipTests
{
    private static FollowRelationship CreateRelationship() =>
        new(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

    private static FollowRelationship CreateMutualFollow()
    {
        var relationship = CreateRelationship();
        relationship.SendRequest(WellKnownUsers.Explorer);
        relationship.SendRequest(WellKnownUsers.SecondExplorer);
        return relationship;
    }

    private static FollowRelationship CreateRelationshipWithStatus(FollowStatus status)
    {
        if (status == FollowStatus.Following)
        {
            return CreateMutualFollow();
        }

        var relationship = CreateRelationship();
        if (status == FollowStatus.Pending)
        {
            relationship.SendRequest(WellKnownUsers.Explorer);
        }
        return relationship;
    }

    [Fact]
    public void A_relationship_cannot_be_created_between_a_user_and_themselves()
    {
        var creation = () => new FollowRelationship(WellKnownUsers.Explorer, WellKnownUsers.Explorer);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_request_someone_they_already_follow()
    {
        var relationship = CreateMutualFollow();

        var sending = () => relationship.SendRequest(WellKnownUsers.Explorer);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_send_a_second_request_while_one_is_pending()
    {
        var relationship = CreateRelationship();
        relationship.SendRequest(WellKnownUsers.Explorer);

        var sending = () => relationship.SendRequest(WellKnownUsers.Explorer);

        sending.Should().Throw<DomainException>();
    }

    [Fact]
    public void Request_is_accepted_at_once_when_the_recipient_already_follows_the_sender()
    {
        var relationship = CreateMutualFollow();
        relationship.Unfollow(WellKnownUsers.Explorer, WellKnownUsers.Explorer);

        var request = relationship.SendRequest(WellKnownUsers.Explorer);

        request.Status.Should().Be(FollowRequestStatus.Accepted);
        relationship.GetFollowStatusFor(WellKnownUsers.Explorer).Should().Be(FollowStatus.Following);
    }

    [Fact]
    public void Crossed_pending_requests_are_both_accepted_and_both_users_follow_each_other()
    {
        var relationship = CreateRelationship();
        relationship.SendRequest(WellKnownUsers.Explorer);

        relationship.SendRequest(WellKnownUsers.SecondExplorer);

        relationship.Requests.Should().OnlyContain(request => request.Status == FollowRequestStatus.Accepted);
        relationship.GetFollowStatusFor(WellKnownUsers.Explorer).Should().Be(FollowStatus.Following);
        relationship.GetFollowStatusFor(WellKnownUsers.SecondExplorer).Should().Be(FollowStatus.Following);
    }

    [Fact]
    public void Sender_cancels_a_pending_request()
    {
        var relationship = CreateRelationship();
        var request = relationship.SendRequest(WellKnownUsers.Explorer);

        relationship.CancelRequest(request.Id, WellKnownUsers.Explorer);

        relationship.Requests.Should().BeEmpty();
    }

    [Fact]
    public void Another_user_cannot_cancel_the_request()
    {
        var relationship = CreateRelationship();
        var request = relationship.SendRequest(WellKnownUsers.Explorer);

        var cancellation = () => relationship.CancelRequest(request.Id, WellKnownUsers.SecondExplorer);

        cancellation.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Accepted_request_cannot_be_cancelled()
    {
        var relationship = CreateMutualFollow();
        var acceptedRequest = relationship.Requests.First(request => request.SenderId == WellKnownUsers.Explorer);

        var cancellation = () => relationship.CancelRequest(acceptedRequest.Id, WellKnownUsers.Explorer);

        cancellation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Unfollowing_ends_the_follow_but_keeps_the_request_history(bool actorIsTheFollower)
    {
        var relationship = CreateMutualFollow();
        var actorId = actorIsTheFollower ? WellKnownUsers.Explorer : WellKnownUsers.SecondExplorer;

        relationship.Unfollow(WellKnownUsers.Explorer, actorId);

        relationship.GetFollowStatusFor(WellKnownUsers.Explorer).Should().Be(FollowStatus.Follow);
        relationship.GetFollowStatusFor(WellKnownUsers.SecondExplorer).Should().Be(FollowStatus.Following);
        relationship.Requests.Should().HaveCount(2);
    }

    [Fact]
    public void Unfollowing_someone_who_is_not_followed_is_rejected()
    {
        var relationship = CreateRelationship();
        relationship.SendRequest(WellKnownUsers.Explorer);

        var unfollowing = () => relationship.Unfollow(WellKnownUsers.Explorer, WellKnownUsers.Explorer);

        unfollowing.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(FollowStatus.Follow)]
    [InlineData(FollowStatus.Pending)]
    [InlineData(FollowStatus.Following)]
    public void Follow_status_reflects_the_relationship_of_the_viewing_user(FollowStatus expected)
    {
        var relationship = CreateRelationshipWithStatus(expected);

        var status = relationship.GetFollowStatusFor(WellKnownUsers.Explorer);

        status.Should().Be(expected);
    }
}

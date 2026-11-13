using FluentAssertions;
using Shared.Domain.Exceptions;
using Social.Domain.Following;
using Xunit;

namespace Social.Tests.Unit.Following;

public class FollowRequestSenderTests
{
    private readonly Guid _senderId = Guid.NewGuid();
    private readonly Guid _recipientId = Guid.NewGuid();
    private readonly FollowRequestSender _sender = new();

    [Fact]
    public void A_request_stays_pending_when_the_recipient_does_not_follow_the_sender()
    {
        var outcome = _sender.Send(_senderId, _recipientId, null, null, null);

        outcome.Request.Status.Should().Be(FollowRequestStatus.Pending);
        outcome.Follow.Should().BeNull();
    }

    [Fact]
    public void A_request_is_accepted_immediately_if_the_recipient_already_follows_the_sender()
    {
        var recipientFollowsSender = new Follow(_recipientId, _senderId);

        var outcome = _sender.Send(_senderId, _recipientId, null, null, recipientFollowsSender);

        outcome.Request.Status.Should().Be(FollowRequestStatus.Accepted);
        outcome.Follow!.FollowerId.Should().Be(_senderId);
        outcome.Follow.FollowedUserId.Should().Be(_recipientId);
    }

    [Fact]
    public void A_user_cannot_send_a_request_to_a_user_they_already_follow()
    {
        var existingFollow = new Follow(_senderId, _recipientId);

        var act = () => _sender.Send(_senderId, _recipientId, existingFollow, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_user_cannot_send_a_second_request_while_one_is_pending()
    {
        var pendingRequest = new FollowRequest(_senderId, _recipientId);

        var act = () => _sender.Send(_senderId, _recipientId, null, pendingRequest, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Following_back_creates_an_accepted_request_and_a_follow_in_the_opposite_direction()
    {
        var acceptedFollow = new Follow(_senderId, _recipientId);

        var outcome = _sender.FollowBack(acceptedFollow, null, null);

        outcome!.Request.SenderId.Should().Be(_recipientId);
        outcome.Request.RecipientId.Should().Be(_senderId);
        outcome.Request.Status.Should().Be(FollowRequestStatus.Accepted);
        outcome.Follow!.FollowerId.Should().Be(_recipientId);
        outcome.Follow.FollowedUserId.Should().Be(_senderId);
    }

    [Fact]
    public void Following_back_creates_nothing_if_the_user_already_follows_the_sender()
    {
        var acceptedFollow = new Follow(_senderId, _recipientId);
        var existingFollowBack = new Follow(_recipientId, _senderId);

        var outcome = _sender.FollowBack(acceptedFollow, existingFollowBack, null);

        outcome.Should().BeNull();
    }

    [Fact]
    public void Following_back_creates_nothing_if_the_user_already_has_a_pending_request_to_the_sender()
    {
        var acceptedFollow = new Follow(_senderId, _recipientId);
        var pendingFollowBack = new FollowRequest(_recipientId, _senderId);

        var outcome = _sender.FollowBack(acceptedFollow, null, pendingFollowBack);

        outcome.Should().BeNull();
    }
}

using FluentAssertions;
using Shared.Domain.Exceptions;
using Social.Domain.Following;
using Xunit;

namespace Social.Tests.Unit.Following;

public class FollowRequestProcessorTests
{
    private readonly Guid _senderId = Guid.NewGuid();
    private readonly Guid _recipientId = Guid.NewGuid();
    private readonly FollowRequestProcessor _processor = new();

    [Fact]
    public void A_request_stays_pending_when_the_recipient_does_not_follow_the_sender()
    {
        var outcome = _processor.Send(_senderId, _recipientId, null, null, null);

        outcome.Request.Status.Should().Be(FollowRequestStatus.Pending);
        outcome.Follow.Should().BeNull();
    }

    [Fact]
    public void A_request_is_accepted_immediately_if_the_recipient_already_follows_the_sender()
    {
        var recipientFollowsSender = new Follow(_recipientId, _senderId);

        var outcome = _processor.Send(_senderId, _recipientId, null, null, recipientFollowsSender);

        outcome.Request.Status.Should().Be(FollowRequestStatus.Accepted);
        outcome.Follow!.FollowerId.Should().Be(_senderId);
        outcome.Follow.FollowedUserId.Should().Be(_recipientId);
    }

    [Fact]
    public void A_user_cannot_send_a_request_to_a_user_they_already_follow()
    {
        var existingFollow = new Follow(_senderId, _recipientId);

        var act = () => _processor.Send(_senderId, _recipientId, existingFollow, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_user_cannot_send_a_second_request_while_one_is_pending()
    {
        var pendingRequest = new FollowRequest(_senderId, _recipientId);

        var act = () => _processor.Send(_senderId, _recipientId, null, pendingRequest, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Accepting_a_request_makes_only_the_sender_follow_when_no_reverse_request_is_pending()
    {
        var request = new FollowRequest(_senderId, _recipientId);

        var follows = _processor.Accept(request, _recipientId, null);

        request.Status.Should().Be(FollowRequestStatus.Accepted);
        follows.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { FollowerId = _senderId, FollowedUserId = _recipientId });
    }

    [Fact]
    public void Accepting_a_request_also_accepts_the_recipients_pending_request_to_the_sender()
    {
        var request = new FollowRequest(_senderId, _recipientId);
        var reversePendingRequest = new FollowRequest(_recipientId, _senderId);

        var follows = _processor.Accept(request, _recipientId, reversePendingRequest);

        request.Status.Should().Be(FollowRequestStatus.Accepted);
        reversePendingRequest.Status.Should().Be(FollowRequestStatus.Accepted);
        follows.Select(follow => (follow.FollowerId, follow.FollowedUserId)).Should().BeEquivalentTo(
            [(_senderId, _recipientId), (_recipientId, _senderId)]);
    }

    [Fact]
    public void A_reverse_pending_request_stays_pending_when_someone_other_than_the_recipient_accepts()
    {
        var request = new FollowRequest(_senderId, _recipientId);
        var reversePendingRequest = new FollowRequest(_recipientId, _senderId);

        var act = () => _processor.Accept(request, Guid.NewGuid(), reversePendingRequest);

        act.Should().Throw<NotFoundException>();
        reversePendingRequest.Status.Should().Be(FollowRequestStatus.Pending);
    }
}

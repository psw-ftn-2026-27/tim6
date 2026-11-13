using FluentAssertions;
using Shared.Domain.Exceptions;
using Social.Domain.Following;
using Xunit;

namespace Social.Tests.Unit.Following;

public class FollowRequestTests
{
    private readonly Guid _senderId = Guid.NewGuid();
    private readonly Guid _recipientId = Guid.NewGuid();

    [Fact]
    public void A_new_request_is_pending()
    {
        var request = new FollowRequest(_senderId, _recipientId);

        request.Status.Should().Be(FollowRequestStatus.Pending);
        request.IsPending.Should().BeTrue();
        request.RespondedAt.Should().BeNull();
    }

    [Fact]
    public void A_user_cannot_send_a_request_to_themselves()
    {
        var act = () => new FollowRequest(_senderId, _senderId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void The_recipient_can_accept_a_pending_request()
    {
        var request = new FollowRequest(_senderId, _recipientId);

        var follow = request.Accept(_recipientId);

        request.Status.Should().Be(FollowRequestStatus.Accepted);
        follow.FollowerId.Should().Be(_senderId);
        follow.FollowedUserId.Should().Be(_recipientId);
        request.RespondedAt.Should().NotBeNull();
    }

    [Fact]
    public void The_recipient_can_reject_a_pending_request()
    {
        var request = new FollowRequest(_senderId, _recipientId);

        request.Reject(_recipientId);

        request.Status.Should().Be(FollowRequestStatus.Rejected);
        request.RespondedAt.Should().NotBeNull();
    }

    [Fact]
    public void The_sender_can_cancel_a_pending_request()
    {
        var request = new FollowRequest(_senderId, _recipientId);

        var act = () => request.EnsureCanBeCancelledBy(_senderId);

        act.Should().NotThrow();
    }

    [Fact]
    public void An_accepted_request_cannot_be_accepted_again()
    {
        var request = CreateAcceptedRequest();

        var act = () => request.Accept(_recipientId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_accepted_request_cannot_be_rejected()
    {
        var request = CreateAcceptedRequest();

        var act = () => request.Reject(_recipientId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_rejected_request_cannot_be_accepted()
    {
        var request = CreateRejectedRequest();

        var act = () => request.Accept(_recipientId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_rejected_request_cannot_be_rejected_again()
    {
        var request = CreateRejectedRequest();

        var act = () => request.Reject(_recipientId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_accepted_request_cannot_be_cancelled()
    {
        var request = CreateAcceptedRequest();

        var act = () => request.EnsureCanBeCancelledBy(_senderId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_rejected_request_cannot_be_cancelled()
    {
        var request = CreateRejectedRequest();

        var act = () => request.EnsureCanBeCancelledBy(_senderId);

        act.Should().Throw<DomainException>();
    }

    private FollowRequest CreateAcceptedRequest()
    {
        var request = new FollowRequest(_senderId, _recipientId);
        request.Accept(_recipientId);
        return request;
    }

    private FollowRequest CreateRejectedRequest()
    {
        var request = new FollowRequest(_senderId, _recipientId);
        request.Reject(_recipientId);
        return request;
    }
}

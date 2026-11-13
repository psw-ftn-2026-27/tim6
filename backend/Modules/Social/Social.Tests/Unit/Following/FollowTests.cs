using FluentAssertions;
using Shared.Domain.Exceptions;
using Social.Domain.Following;
using Xunit;

namespace Social.Tests.Unit.Following;

public class FollowTests
{
    [Fact]
    public void A_follow_records_who_follows_whom()
    {
        var followerId = Guid.NewGuid();
        var followedUserId = Guid.NewGuid();

        var follow = new Follow(followerId, followedUserId);

        follow.FollowerId.Should().Be(followerId);
        follow.FollowedUserId.Should().Be(followedUserId);
    }

    [Fact]
    public void A_user_cannot_follow_themselves()
    {
        var userId = Guid.NewGuid();

        var act = () => new Follow(userId, userId);

        act.Should().Throw<DomainException>();
    }
}

using FluentAssertions;
using Shared.Domain.Conversations;
using Shared.Domain.Exceptions;
using Xunit;

namespace Shared.Conversations.Tests.Unit.Conversations;

public class ConversationTests
{
    private static readonly DateTime Now = new(2026, 11, 19, 10, 0, 0, DateTimeKind.Utc);
    private static readonly Guid FirstUser = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SecondUser = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ThirdUser = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void direct_conversation_has_two_different_equal_participants()
    {
        var conversation = Conversation.CreateDirect(FirstUser, SecondUser);

        conversation.Participants.Should().HaveCount(2);
        conversation.Participants.Should().OnlyContain(participant => participant.Role == ConversationRole.Participant);
        conversation.Participants.Select(participant => participant.UserId)
            .Should().BeEquivalentTo([FirstUser, SecondUser]);
    }

    [Fact]
    public void direct_conversation_rejects_the_same_user_twice()
    {
        var creation = () => Conversation.CreateDirect(FirstUser, FirstUser);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void direct_conversation_rejects_a_third_participant()
    {
        var conversation = Conversation.CreateDirect(FirstUser, SecondUser);

        var addition = () => conversation.AddParticipant(ThirdUser);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void contextual_conversation_keeps_roles_and_rejects_duplicate_participants()
    {
        var conversation = CreateContextualConversation();
        conversation.AddParticipant(SecondUser);

        var duplicate = () => conversation.AddParticipant(SecondUser);

        conversation.Participants.Single(participant => participant.UserId == FirstUser).Role
            .Should().Be(ConversationRole.Privileged);
        conversation.Participants.Single(participant => participant.UserId == SecondUser).Role
            .Should().Be(ConversationRole.Participant);
        duplicate.Should().Throw<DomainException>();
    }

    [Fact]
    public void outsider_cannot_create_a_message()
    {
        var conversation = Conversation.CreateDirect(FirstUser, SecondUser);

        var creation = () => conversation.CreateMessage(ThirdUser, "Hello", Now);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void message_requires_content(string content)
    {
        var conversation = Conversation.CreateDirect(FirstUser, SecondUser);

        var creation = () => conversation.CreateMessage(FirstUser, content, Now);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void author_can_delete_own_message_only_once()
    {
        var conversation = Conversation.CreateDirect(FirstUser, SecondUser);
        var message = conversation.CreateMessage(FirstUser, "Hello", Now);
        conversation.EnsureCanDeleteMessage(FirstUser, message);
        message.Delete(Now.AddMinutes(1));

        var repeatedDeletion = () => message.Delete(Now.AddMinutes(2));

        message.Content.Should().BeNull();
        message.DeletedAt.Should().Be(Now.AddMinutes(1));
        repeatedDeletion.Should().Throw<DomainException>();
    }

    [Fact]
    public void ordinary_participant_cannot_delete_another_participants_message()
    {
        var conversation = Conversation.CreateDirect(FirstUser, SecondUser);
        var message = conversation.CreateMessage(FirstUser, "Hello", Now);

        var deletion = () => conversation.EnsureCanDeleteMessage(SecondUser, message);

        deletion.Should().Throw<DomainException>();
    }

    [Fact]
    public void privileged_participant_can_delete_another_participants_message()
    {
        var conversation = CreateContextualConversation();
        conversation.AddParticipant(SecondUser);
        var message = conversation.CreateMessage(SecondUser, "Hello", Now);

        var permission = () => conversation.EnsureCanDeleteMessage(FirstUser, message);

        permission.Should().NotThrow();
    }

    [Theory]
    [InlineData(MuteDuration.FifteenMinutes)]
    [InlineData(MuteDuration.OneHour)]
    [InlineData(MuteDuration.TwentyFourHours)]
    [InlineData(MuteDuration.Permanent)]
    public void privileged_participant_can_mute_an_ordinary_participant(MuteDuration duration)
    {
        var conversation = CreateContextualConversation();
        conversation.AddParticipant(SecondUser);

        conversation.Mute(FirstUser, SecondUser, duration, Now);

        conversation.Participants.Single(participant => participant.UserId == SecondUser).Mute
            .Should().NotBeNull();
    }

    [Fact]
    public void unsupported_mute_duration_is_rejected_by_the_domain()
    {
        var conversation = CreateContextualConversation();
        conversation.AddParticipant(SecondUser);

        var muting = () => conversation.Mute(FirstUser, SecondUser, (MuteDuration)999, Now);

        muting.Should().Throw<DomainException>();
    }

    [Fact]
    public void ordinary_participant_cannot_mute_and_privileged_participant_cannot_be_muted()
    {
        var conversation = CreateContextualConversation();
        conversation.AddParticipant(SecondUser);

        var ordinaryModerator = () => conversation.Mute(SecondUser, FirstUser, MuteDuration.OneHour, Now);
        var privilegedTarget = () => conversation.Mute(FirstUser, FirstUser, MuteDuration.OneHour, Now);

        ordinaryModerator.Should().Throw<DomainException>();
        privilegedTarget.Should().Throw<DomainException>();
    }

    [Fact]
    public void timed_mute_blocks_sending_until_it_expires()
    {
        var conversation = CreateContextualConversation();
        conversation.AddParticipant(SecondUser);
        conversation.Mute(FirstUser, SecondUser, MuteDuration.FifteenMinutes, Now);

        var whileMuted = () => conversation.CreateMessage(SecondUser, "Blocked", Now.AddMinutes(14));
        var afterExpiry = () => conversation.CreateMessage(SecondUser, "Allowed", Now.AddMinutes(15));

        whileMuted.Should().Throw<DomainException>();
        afterExpiry.Should().NotThrow();
        conversation.CanBeReadBy(SecondUser).Should().BeTrue();
    }

    [Fact]
    public void privileged_participant_can_remove_a_mute_early()
    {
        var conversation = CreateContextualConversation();
        conversation.AddParticipant(SecondUser);
        conversation.Mute(FirstUser, SecondUser, MuteDuration.Permanent, Now);

        conversation.Unmute(FirstUser, SecondUser);

        var sending = () => conversation.CreateMessage(SecondUser, "Allowed", Now.AddMinutes(1));
        sending.Should().NotThrow();
    }

    private static Conversation CreateContextualConversation() => Conversation.CreateContextual(
        ConversationContext.Contextual("tour", Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
        [FirstUser]);
}

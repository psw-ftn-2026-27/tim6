using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourConversationMessaging;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain.Conversations;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourConversationMessaging;

public class TourConversationMessagingCommandTests : BaseIntegrationTest
{
    public TourConversationMessagingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task author_opens_the_existing_tour_conversation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "author");

        var response = await client.PostAsync(
            $"/api/exploration/conversations/tour/{TourSeed.FortressWalk.Id}", null);

        response.EnsureSuccessStatusCode();
        (await response.Content.ReadFromJsonAsync<Guid>()).Should().Be(ConversationSeed.TourConversation.Id);
        using var context = Factory.CreateContext<ExplorationDbContext>();
        context.Conversations.ToList().Count(value => value.Context.ContextId == TourSeed.FortressWalk.Id)
            .Should().Be(1);
    }

    [Fact]
    public async Task buyer_joins_the_tour_conversation_as_an_ordinary_participant()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/exploration/conversations/tour/{TourSeed.PublishedVineyards.Id}", null);

        response.EnsureSuccessStatusCode();
        using var context = Factory.CreateContext<ExplorationDbContext>();
        var conversation = context.Conversations.ToList()
            .Single(value => value.Context.ContextId == TourSeed.PublishedVineyards.Id);
        conversation.Participants.Single(value => value.UserId == WellKnownUsers.SecondExplorer)
            .Role.Should().Be(ConversationRole.Participant);
    }

    [Fact]
    public async Task user_who_has_not_purchased_the_tour_cannot_open_its_conversation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "explorer");

        var response = await client.PostAsync(
            $"/api/exploration/conversations/tour/{TourSeed.PublishedVineyards.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<ExplorationDbContext>();
        context.Conversations.ToList().Should().NotContain(value =>
            value.Context.ContextId == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task participant_sends_a_message_that_is_stored_with_their_identity()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/exploration/conversations/{ConversationSeed.TourConversation.Id}/messages",
            new SendMessageDto("Nova poruka"));

        response.EnsureSuccessStatusCode();
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        using var context = Factory.CreateContext<ExplorationDbContext>();
        context.ChatMessages.Single(message => message.Id == id).AuthorId.Should().Be(WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task author_can_mute_an_unprivileged_participant()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "author");

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/conversations/{ConversationSeed.TourConversation.Id}/mute",
            new MuteParticipantDto(WellKnownUsers.SecondExplorer, MuteDuration.OneHour));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ExplorationDbContext>();
        context.Conversations.Single(value => value.Id == ConversationSeed.TourConversation.Id).Participants
            .Single(participant => participant.UserId == WellKnownUsers.SecondExplorer).Mute.Should().NotBeNull();
    }

    [Fact]
    public async Task ordinary_participant_cannot_mute_another_participant()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PutAsJsonAsync(
            $"/api/exploration/conversations/{ConversationSeed.TourConversation.Id}/mute",
            new MuteParticipantDto(WellKnownUsers.Explorer, MuteDuration.OneHour));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task author_can_unmute_a_muted_participant()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "author");

        var response = await client.DeleteAsync(
            $"/api/exploration/conversations/{ConversationSeed.MutedTourConversation.Id}" +
            $"/mute/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ExplorationDbContext>();
        context.Conversations.Single(value => value.Id == ConversationSeed.MutedTourConversation.Id)
            .Participants.Single(value => value.UserId == WellKnownUsers.SecondExplorer)
            .Mute.Should().BeNull();
    }

    [Fact]
    public async Task ordinary_participant_cannot_unmute_a_participant()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/exploration/conversations/{ConversationSeed.MutedTourConversation.Id}" +
            $"/mute/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task permanently_muted_participant_can_read_messages()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/exploration/conversations/{ConversationSeed.PermanentlyMutedTourConversation.Id}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task permanently_muted_participant_cannot_send_messages()
    {
        const string content = "Poruka utišanog učesnika";
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/exploration/conversations/{ConversationSeed.PermanentlyMutedTourConversation.Id}/messages",
            new SendMessageDto(content));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<ExplorationDbContext>();
        context.ChatMessages.ToList().Should().NotContain(message =>
            message.Content != null && message.Content.Value == content);
    }

    [Fact]
    public async Task privileged_participant_deletes_another_participants_message()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "author");

        var response = await client.DeleteAsync(
            $"/api/exploration/conversations/messages/{ConversationSeed.BuyersMessage.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ExplorationDbContext>();
        var stored = context.ChatMessages.Single(message => message.Id == ConversationSeed.BuyersMessage.Id);
        stored.Content.Should().BeNull();
        stored.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task ordinary_participant_cannot_delete_another_participants_message()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/exploration/conversations/messages/{ConversationSeed.AuthorsMessage.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task unauthenticated_user_cannot_send_to_a_tour_conversation()
    {
        var response = await Factory.CreateClient().PostAsJsonAsync(
            $"/api/exploration/conversations/{ConversationSeed.TourConversation.Id}/messages",
            new SendMessageDto("Nedozvoljena poruka"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task unauthenticated_user_cannot_negotiate_a_realtime_connection()
    {
        var response = await Factory.CreateClient().PostAsync(
            "/hubs/exploration/conversations/negotiate?negotiateVersion=1", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

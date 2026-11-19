using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain.Conversations;
using Shared.Tests;
using Social.Application.DirectConversationMessaging;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.DirectConversationMessaging;

public class DirectConversationMessagingCommandTests : BaseIntegrationTest
{
    public DirectConversationMessagingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task opening_the_same_direct_conversation_returns_the_existing_conversation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/conversations/direct/{WellKnownUsers.SecondExplorer}", null);

        response.EnsureSuccessStatusCode();
        (await response.Content.ReadFromJsonAsync<Guid>()).Should().Be(ConversationSeed.Direct.Id);
        using var context = Factory.CreateContext<SocialDbContext>();
        context.Conversations.Should().ContainSingle();
    }

    [Fact]
    public async Task opening_a_direct_conversation_with_self_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/conversations/direct/{WellKnownUsers.Explorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task opening_a_direct_conversation_with_a_missing_user_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/conversations/direct/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task participant_sends_a_message_that_is_stored_with_their_identity()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/social/conversations/{ConversationSeed.Direct.Id}/messages",
            new SendMessageDto("Nova poruka"));

        response.EnsureSuccessStatusCode();
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        using var context = Factory.CreateContext<SocialDbContext>();
        context.ChatMessages.Single(message => message.Id == id).AuthorId.Should().Be(WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task outsider_cannot_send_to_a_direct_conversation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/social/conversations/{ConversationSeed.Direct.Id}/messages",
            new SendMessageDto("Nedozvoljena poruka"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task empty_message_is_rejected_without_being_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var beforeContext = Factory.CreateContext<SocialDbContext>();
        var before = beforeContext.ChatMessages.Count();

        var response = await client.PostAsJsonAsync(
            $"/api/social/conversations/{ConversationSeed.Direct.Id}/messages",
            new SendMessageDto("   "));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var afterContext = Factory.CreateContext<SocialDbContext>();
        afterContext.ChatMessages.Should().HaveCount(before);
    }

    [Fact]
    public async Task unauthenticated_user_cannot_send_to_a_direct_conversation()
    {
        var response = await Factory.CreateClient().PostAsJsonAsync(
            $"/api/social/conversations/{ConversationSeed.Direct.Id}/messages",
            new SendMessageDto("Nedozvoljena poruka"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task unauthenticated_user_cannot_negotiate_a_realtime_connection()
    {
        var response = await Factory.CreateClient().PostAsync(
            "/hubs/social/conversations/negotiate?negotiateVersion=1", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task authenticated_realtime_connection_accepts_the_signalr_query_token()
    {
        var token = Factory.CreateAccessToken(WellKnownUsers.Explorer, "explorer");

        var response = await Factory.CreateClient().PostAsync(
            $"/hubs/social/conversations/negotiate?negotiateVersion=1&access_token={Uri.EscapeDataString(token)}",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task author_deletes_their_message_without_leaving_its_content()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/social/conversations/messages/{ConversationSeed.ExplorersMessage.Id}");
        var messages = await client.GetFromJsonAsync<Social.Application.Conversations.MessagePageDto>(
            $"/api/social/conversations/{ConversationSeed.Direct.Id}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        messages!.Items.Single(value => value.Id == ConversationSeed.ExplorersMessage.Id)
            .Content.Should().BeNull();
        using var context = Factory.CreateContext<SocialDbContext>();
        var stored = context.ChatMessages.Single(message => message.Id == ConversationSeed.ExplorersMessage.Id);
        stored.Content.Should().BeNull();
        stored.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task ordinary_participant_cannot_delete_another_participants_message()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/social/conversations/messages/{ConversationSeed.ExplorersMessage.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task deleted_message_cannot_be_deleted_again()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var url = $"/api/social/conversations/messages/{ConversationSeed.ExplorersMessage.Id}";

        (await client.DeleteAsync(url)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.DeleteAsync(url)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

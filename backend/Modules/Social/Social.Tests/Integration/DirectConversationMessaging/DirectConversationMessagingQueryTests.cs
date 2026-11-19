using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.Conversations;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.DirectConversationMessaging;

public class DirectConversationMessagingQueryTests : BaseIntegrationTest
{
    public DirectConversationMessagingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task participant_reads_the_conversation_and_its_messages()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var conversation = await client.GetFromJsonAsync<ConversationDto>(
            $"/api/social/conversations/{ConversationSeed.Direct.Id}", JsonOptions);
        var messages = await client.GetFromJsonAsync<MessagePageDto>(
            $"/api/social/conversations/{ConversationSeed.Direct.Id}/messages", JsonOptions);

        conversation!.Id.Should().Be(ConversationSeed.Direct.Id);
        messages!.Items.Should().Contain(message => message.Id == ConversationSeed.ExplorersMessage.Id);
    }

    [Fact]
    public async Task outsider_cannot_read_another_conversations_history()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "explorer");

        var response = await client.GetAsync(
            $"/api/social/conversations/{ConversationSeed.Direct.Id}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task user_receives_only_conversations_they_participate_in()
    {
        var participant = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var outsider = Factory.CreateClientFor(WellKnownUsers.Administrator, "explorer");

        var mine = await participant.GetFromJsonAsync<Shared.Domain.PageResult<ConversationSummaryDto>>(
            "/api/social/conversations/mine", JsonOptions);
        var outsiders = await outsider.GetFromJsonAsync<Shared.Domain.PageResult<ConversationSummaryDto>>(
            "/api/social/conversations/mine", JsonOptions);

        mine!.Items.Should().ContainSingle(value => value.Id == ConversationSeed.Direct.Id);
        outsiders!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task cursors_load_older_messages_and_catch_up_newer_messages_without_duplicates()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var baseUrl = $"/api/social/conversations/{ConversationSeed.Direct.Id}/messages";

        var latest = await client.GetFromJsonAsync<MessagePageDto>($"{baseUrl}?pageSize=1", JsonOptions);
        var latestCursor = latest!.NextCursor!;
        var older = await client.GetFromJsonAsync<MessagePageDto>(
            $"{baseUrl}?pageSize=1&cursorCreatedAt={Uri.EscapeDataString(latestCursor.CreatedAt.ToString("O"))}&cursorId={latestCursor.Id}",
            JsonOptions);
        var olderCursor = older!.NextCursor!;
        var caughtUp = await client.GetFromJsonAsync<MessagePageDto>(
            $"{baseUrl}?pageSize=10&afterCursor=true&cursorCreatedAt={Uri.EscapeDataString(olderCursor.CreatedAt.ToString("O"))}&cursorId={olderCursor.Id}",
            JsonOptions);

        latest.Items.Should().ContainSingle(message => message.Id == ConversationSeed.SecondExplorersMessage.Id);
        older.Items.Should().ContainSingle(message => message.Id == ConversationSeed.ExplorersMessage.Id);
        caughtUp!.Items.Should().ContainSingle(message => message.Id == ConversationSeed.SecondExplorersMessage.Id);
    }
}

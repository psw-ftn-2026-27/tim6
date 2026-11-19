using System.Net;
using System.Net.Http.Json;
using Exploration.Application.Conversations;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourConversationMessaging;

public class TourConversationMessagingQueryTests : BaseIntegrationTest
{
    public TourConversationMessagingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task participant_reads_the_tour_conversation_and_its_messages()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var conversation = await client.GetFromJsonAsync<ConversationDto>(
            $"/api/exploration/conversations/{ConversationSeed.TourConversation.Id}", JsonOptions);
        var messages = await client.GetFromJsonAsync<MessagePageDto>(
            $"/api/exploration/conversations/{ConversationSeed.TourConversation.Id}/messages", JsonOptions);

        conversation!.Id.Should().Be(ConversationSeed.TourConversation.Id);
        messages!.Items.Should().Contain(message => message.Id == ConversationSeed.BuyersMessage.Id);
    }

    [Fact]
    public async Task outsider_cannot_read_a_tour_conversations_history()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "explorer");

        var response = await client.GetAsync(
            $"/api/exploration/conversations/{ConversationSeed.TourConversation.Id}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task reconnect_cursor_returns_only_messages_after_the_last_seen_message()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var cursor = ConversationSeed.BuyersMessage;
        var createdAt = Uri.EscapeDataString(cursor.CreatedAt.ToString("O"));

        var messages = await client.GetFromJsonAsync<MessagePageDto>(
            $"/api/exploration/conversations/{ConversationSeed.TourConversation.Id}/messages" +
            $"?afterCursor=true&cursorCreatedAt={createdAt}&cursorId={cursor.Id}", JsonOptions);

        messages!.Items.Should().ContainSingle(message => message.Id == ConversationSeed.AuthorsMessage.Id);
    }
}

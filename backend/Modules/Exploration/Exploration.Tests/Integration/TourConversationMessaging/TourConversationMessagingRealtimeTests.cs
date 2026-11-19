using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourConversationMessaging;

public class TourConversationMessagingRealtimeTests : BaseIntegrationTest
{
    private const char RecordSeparator = '\u001e';

    public TourConversationMessagingRealtimeTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task participant_joins_the_conversation_realtime_group()
    {
        using var socket = await ConnectAsync(WellKnownUsers.SecondExplorer);

        var completion = await JoinConversationAsync(socket, ConversationSeed.TourConversation.Id);

        using var message = ParseMessage(completion);
        message.RootElement.GetProperty("type").GetInt32().Should().Be(3);
        message.RootElement.TryGetProperty("error", out _).Should().BeFalse();
    }

    [Fact]
    public async Task outsider_cannot_join_the_conversation_realtime_group()
    {
        using var socket = await ConnectAsync(WellKnownUsers.Administrator);

        var completion = await JoinConversationAsync(socket, ConversationSeed.TourConversation.Id);

        using var message = ParseMessage(completion);
        message.RootElement.GetProperty("type").GetInt32().Should().Be(3);
        message.RootElement.TryGetProperty("error", out _).Should().BeTrue();
    }

    private async Task<WebSocket> ConnectAsync(Guid userId)
    {
        var token = Factory.CreateAccessToken(userId, "explorer");
        var client = Factory.Server.CreateWebSocketClient();
        var socket = await client.ConnectAsync(new Uri(
            $"ws://localhost/hubs/exploration/conversations?access_token={Uri.EscapeDataString(token)}"),
            CancellationToken.None);
        await SendAsync(socket, "{\"protocol\":\"json\",\"version\":1}" + RecordSeparator);
        await ReceiveAsync(socket);
        return socket;
    }

    private static async Task<string> JoinConversationAsync(WebSocket socket, Guid conversationId)
    {
        var invocation = JsonSerializer.Serialize(new
        {
            type = 1,
            invocationId = "1",
            target = "JoinConversation",
            arguments = new[] { conversationId }
        });
        await SendAsync(socket, invocation + RecordSeparator);
        return await ReceiveAsync(socket);
    }

    private static async Task SendAsync(WebSocket socket, string message)
    {
        var payload = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task<string> ReceiveAsync(WebSocket socket)
    {
        var buffer = new byte[4096];
        using var stream = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            stream.Write(buffer, 0, result.Count);
        }
        while (!result.EndOfMessage);

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static JsonDocument ParseMessage(string message) =>
        JsonDocument.Parse(message.TrimEnd(RecordSeparator));
}

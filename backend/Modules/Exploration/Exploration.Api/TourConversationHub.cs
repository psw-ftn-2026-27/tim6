using Exploration.Application.TourConversationMessaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Api;

namespace Exploration.Api;

[Authorize]
public sealed class TourConversationHub : Hub
{
    internal static string Group(Guid conversationId) => $"exploration-conversation-{conversationId}";
    private readonly TourConversationMessagingQueries _queries;
    public TourConversationHub(TourConversationMessagingQueries queries) => _queries = queries;

    public async Task JoinConversation(Guid conversationId)
    {
        await _queries.EnsureAccessAsync(Context.User!.GetUserId(), conversationId);
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(conversationId));
    }
}

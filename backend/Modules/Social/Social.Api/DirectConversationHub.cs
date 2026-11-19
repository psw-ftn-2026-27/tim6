using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Api;
using Social.Application.DirectConversationMessaging;

namespace Social.Api;

[Authorize]
public sealed class DirectConversationHub : Hub
{
    internal static string Group(Guid conversationId) => $"social-conversation-{conversationId}";

    private readonly DirectConversationMessagingQueries _queries;
    public DirectConversationHub(DirectConversationMessagingQueries queries) => _queries = queries;

    public async Task JoinConversation(Guid conversationId)
    {
        await _queries.EnsureAccessAsync(Context.User!.GetUserId(), conversationId);
        await Groups.AddToGroupAsync(Context.ConnectionId, Group(conversationId));
    }
}

using Microsoft.AspNetCore.SignalR;
using Shared.Api.Conversations;
using Social.Application.DirectConversationMessaging;

namespace Social.Api;

internal sealed class SignalRConversationNotifier : IConversationNotifier
{
    private readonly IHubContext<DirectConversationHub> _hub;
    public SignalRConversationNotifier(IHubContext<DirectConversationHub> hub) => _hub = hub;
    public Task MessageSentAsync(Guid id) => Send(id, ConversationChangeType.MessageSent);
    public Task MessageDeletedAsync(Guid id) => Send(id, ConversationChangeType.MessageDeleted);
    private Task Send(Guid id, ConversationChangeType type) => _hub.Clients
        .Group(DirectConversationHub.Group(id)).SendAsync("ConversationChanged", new ConversationChangedEvent(id, type));
}

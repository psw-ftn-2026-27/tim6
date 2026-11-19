using Exploration.Application.TourConversationMessaging;
using Microsoft.AspNetCore.SignalR;
using Shared.Api.Conversations;

namespace Exploration.Api;

internal sealed class SignalRConversationNotifier : IConversationNotifier
{
    private readonly IHubContext<TourConversationHub> _hub;
    public SignalRConversationNotifier(IHubContext<TourConversationHub> hub) => _hub = hub;
    public Task MessageSentAsync(Guid id) => Send(id, ConversationChangeType.MessageSent);
    public Task MessageDeletedAsync(Guid id) => Send(id, ConversationChangeType.MessageDeleted);
    public Task ParticipantMutedAsync(Guid id) => Send(id, ConversationChangeType.ParticipantMuted);
    public Task ParticipantUnmutedAsync(Guid id) => Send(id, ConversationChangeType.ParticipantUnmuted);
    private Task Send(Guid id, ConversationChangeType type) => _hub.Clients
        .Group(TourConversationHub.Group(id)).SendAsync("ConversationChanged", new ConversationChangedEvent(id, type));
}

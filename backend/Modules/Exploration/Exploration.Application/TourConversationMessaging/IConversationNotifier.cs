namespace Exploration.Application.TourConversationMessaging;

public interface IConversationNotifier
{
    Task MessageSentAsync(Guid conversationId);
    Task MessageDeletedAsync(Guid conversationId);
    Task ParticipantMutedAsync(Guid conversationId);
    Task ParticipantUnmutedAsync(Guid conversationId);
}

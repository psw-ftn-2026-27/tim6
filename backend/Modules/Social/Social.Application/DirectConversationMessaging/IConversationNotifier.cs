namespace Social.Application.DirectConversationMessaging;

public interface IConversationNotifier
{
    Task MessageSentAsync(Guid conversationId);
    Task MessageDeletedAsync(Guid conversationId);
}

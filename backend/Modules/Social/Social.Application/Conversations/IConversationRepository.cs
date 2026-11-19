using Shared.Domain.Conversations;

namespace Social.Application.Conversations;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id);
    Task<Conversation?> GetDirectAsync(Guid firstUserId, Guid secondUserId);
    void Add(Conversation conversation);
}

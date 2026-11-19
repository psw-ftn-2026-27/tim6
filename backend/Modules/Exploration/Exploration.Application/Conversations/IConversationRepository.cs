using Shared.Domain.Conversations;

namespace Exploration.Application.Conversations;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id);
    Task<Conversation?> GetByContextAsync(string contextType, Guid contextId);
    void Add(Conversation conversation);
}

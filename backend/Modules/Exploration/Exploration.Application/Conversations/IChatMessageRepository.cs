using Shared.Domain.Conversations;

namespace Exploration.Application.Conversations;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByIdAsync(Guid id);
    void Add(ChatMessage message);
}

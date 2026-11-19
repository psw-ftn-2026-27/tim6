using Shared.Domain.Conversations;

namespace Social.Application.Conversations;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByIdAsync(Guid id);
    void Add(ChatMessage message);
}

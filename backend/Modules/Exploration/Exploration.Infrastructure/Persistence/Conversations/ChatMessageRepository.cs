using Exploration.Application.Conversations;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Conversations;

namespace Exploration.Infrastructure.Persistence.Conversations;

internal sealed class ChatMessageRepository : IChatMessageRepository
{
    private readonly ExplorationDbContext _dbContext;
    public ChatMessageRepository(ExplorationDbContext dbContext) => _dbContext = dbContext;
    public Task<ChatMessage?> GetByIdAsync(Guid id) =>
        _dbContext.ChatMessages.FirstOrDefaultAsync(message => message.Id == id);
    public void Add(ChatMessage message) => _dbContext.ChatMessages.Add(message);
}

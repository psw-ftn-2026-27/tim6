using Exploration.Application.Conversations;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Conversations;

namespace Exploration.Infrastructure.Persistence.Conversations;

internal sealed class ConversationRepository : IConversationRepository
{
    private readonly ExplorationDbContext _dbContext;
    public ConversationRepository(ExplorationDbContext dbContext) => _dbContext = dbContext;
    public Task<Conversation?> GetByIdAsync(Guid id) =>
        _dbContext.Conversations.FirstOrDefaultAsync(conversation => conversation.Id == id);
    public Task<Conversation?> GetByContextAsync(string contextType, Guid contextId) =>
        _dbContext.Conversations.FirstOrDefaultAsync(conversation =>
            conversation.Context.Type == contextType && conversation.Context.ContextId == contextId);
    public void Add(Conversation conversation) => _dbContext.Conversations.Add(conversation);
}

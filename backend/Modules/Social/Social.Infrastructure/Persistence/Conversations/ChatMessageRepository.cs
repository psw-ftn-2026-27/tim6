using Microsoft.EntityFrameworkCore;
using Shared.Domain.Conversations;
using Social.Application.Conversations;

namespace Social.Infrastructure.Persistence.Conversations;

internal sealed class ChatMessageRepository : IChatMessageRepository
{
    private readonly SocialDbContext _dbContext;
    public ChatMessageRepository(SocialDbContext dbContext) => _dbContext = dbContext;
    public Task<ChatMessage?> GetByIdAsync(Guid id) =>
        _dbContext.ChatMessages.FirstOrDefaultAsync(message => message.Id == id);
    public void Add(ChatMessage message) => _dbContext.ChatMessages.Add(message);
}

using Microsoft.EntityFrameworkCore;
using Shared.Domain.Conversations;
using Social.Application.Conversations;

namespace Social.Infrastructure.Persistence.Conversations;

internal sealed class ConversationRepository : IConversationRepository
{
    private readonly SocialDbContext _dbContext;
    public ConversationRepository(SocialDbContext dbContext) => _dbContext = dbContext;

    public Task<Conversation?> GetByIdAsync(Guid id) =>
        _dbContext.Conversations.FirstOrDefaultAsync(conversation => conversation.Id == id);

    public Task<Conversation?> GetDirectAsync(Guid firstUserId, Guid secondUserId) =>
        _dbContext.Conversations.FirstOrDefaultAsync(conversation =>
            conversation.Context.Type == "direct" &&
            conversation.Participants.Any(participant => participant.UserId == firstUserId) &&
            conversation.Participants.Any(participant => participant.UserId == secondUserId));

    public void Add(Conversation conversation) => _dbContext.Conversations.Add(conversation);
}

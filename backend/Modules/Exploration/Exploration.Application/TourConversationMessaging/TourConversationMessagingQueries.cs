using Exploration.Application.Conversations;
using Exploration.Application.Tours;
using Identity.Contracts;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourConversationMessaging;

public sealed class TourConversationMessagingQueries
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationReadRepository _readRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IIdentityApi _identityApi;

    public TourConversationMessagingQueries(
        IConversationRepository conversationRepository,
        IConversationReadRepository readRepository,
        ITourRepository tourRepository,
        IIdentityApi identityApi)
    {
        _conversationRepository = conversationRepository;
        _readRepository = readRepository;
        _tourRepository = tourRepository;
        _identityApi = identityApi;
    }

    public async Task<ConversationDto> GetConversationAsync(Guid userId, Guid conversationId)
    {
        await EnsureAccessAsync(userId, conversationId);
        var conversation = await _readRepository.GetAsync(conversationId)
            ?? throw new NotFoundException($"Conversation {conversationId} does not exist.");
        var tour = conversation.ContextId is null
            ? null : await _tourRepository.GetByIdAsync(conversation.ContextId.Value);
        if (tour is null)
        {
            throw new NotFoundException("The tour for this conversation does not exist.");
        }
        var users = await _identityApi.GetUsersByIdsAsync(
            conversation.Participants.Select(participant => participant.UserId));
        return conversation with
        {
            TourName = tour.Name,
            Participants = conversation.Participants.Select(participant => participant with
            {
                UserEmail = users.TryGetValue(participant.UserId, out var user) ? user.Email : null
            }).ToList()
        };
    }

    public async Task<MessagePageDto> GetMessagesAsync(
        Guid userId, Guid conversationId, MessageCursorDto? cursor, bool afterCursor, int pageSize)
    {
        await EnsureAccessAsync(userId, conversationId);
        return await _readRepository.GetMessagesAsync(
            conversationId, cursor, afterCursor, Math.Clamp(pageSize, 1, 100));
    }

    public async Task EnsureAccessAsync(Guid userId, Guid conversationId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation is null || !conversation.CanBeReadBy(userId))
        {
            throw new NotFoundException($"Conversation {conversationId} does not exist.");
        }
    }
}

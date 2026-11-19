using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Conversations;

namespace Social.Application.DirectConversationMessaging;

public sealed class DirectConversationMessagingQueries
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationReadRepository _readRepository;
    private readonly IIdentityApi _identityApi;

    public DirectConversationMessagingQueries(
        IConversationRepository conversationRepository,
        IConversationReadRepository readRepository,
        IIdentityApi identityApi)
    {
        _conversationRepository = conversationRepository;
        _readRepository = readRepository;
        _identityApi = identityApi;
    }

    public async Task<Social.Application.Conversations.UserSearchPageDto> SearchUsersAsync(
        string searchTerm, int page, int pageSize)
    {
        var result = await _identityApi.SearchUsersAsync(searchTerm, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
        return new Social.Application.Conversations.UserSearchPageDto(
            result.Items.Select(user => new ConversationUserDto(user.UserId, user.Email)).ToList(),
            result.TotalCount);
    }

    public async Task<PageResult<ConversationSummaryDto>> GetMineAsync(
        Guid userId, int page, int pageSize)
    {
        var conversations = await _readRepository.GetMineAsync(
            userId, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
        var otherUserIds = conversations.Items.SelectMany(conversation =>
            conversation.ParticipantIds.Where(participantId => participantId != userId));
        var users = await _identityApi.GetUsersByIdsAsync(otherUserIds);
        return new PageResult<ConversationSummaryDto>(conversations.Items.Select(conversation =>
        {
            var otherUserId = conversation.ParticipantIds.Single(id => id != userId);
            return conversation with
            {
                OtherParticipantEmail = users.TryGetValue(otherUserId, out var user)
                    ? user.Email : null
            };
        }).ToList(), conversations.TotalCount);
    }

    public async Task<ConversationDto> GetConversationAsync(Guid userId, Guid conversationId)
    {
        await EnsureAccessAsync(userId, conversationId);
        var conversation = await _readRepository.GetAsync(conversationId)
            ?? throw new NotFoundException($"Conversation {conversationId} does not exist.");
        var users = await _identityApi.GetUsersByIdsAsync(
            conversation.Participants.Select(participant => participant.UserId));
        return conversation with
        {
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

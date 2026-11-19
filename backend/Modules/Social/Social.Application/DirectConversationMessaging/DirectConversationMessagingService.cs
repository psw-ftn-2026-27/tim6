using Identity.Contracts;
using Shared.Domain.Conversations;
using Shared.Domain.Exceptions;
using Social.Application.Conversations;

namespace Social.Application.DirectConversationMessaging;

public sealed class DirectConversationMessagingService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IIdentityApi _identityApi;
    private readonly IConversationNotifier _notifier;
    private readonly IUnitOfWork _unitOfWork;

    public DirectConversationMessagingService(
        IConversationRepository conversationRepository,
        IChatMessageRepository messageRepository,
        IIdentityApi identityApi,
        IConversationNotifier notifier,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _identityApi = identityApi;
        _notifier = notifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> OpenDirectAsync(Guid userId, Guid otherUserId)
    {
        var candidate = Conversation.CreateDirect(userId, otherUserId);
        var existing = await _conversationRepository.GetDirectAsync(userId, otherUserId);
        if (existing is not null)
        {
            return existing.Id;
        }

        if (await _identityApi.GetUserByIdAsync(otherUserId) is null)
        {
            throw new NotFoundException($"User {otherUserId} does not exist.");
        }

        _conversationRepository.Add(candidate);
        await _unitOfWork.SaveChangesAsync();
        return candidate.Id;
    }

    public async Task<Guid> SendAsync(Guid userId, Guid conversationId, string content)
    {
        var conversation = await RequireConversation(conversationId);
        var message = conversation.CreateMessage(userId, content, DateTime.UtcNow);
        _messageRepository.Add(message);
        await _unitOfWork.SaveChangesAsync();
        await _notifier.MessageSentAsync(conversationId);
        return message.Id;
    }

    public async Task DeleteAsync(Guid userId, Guid messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId)
            ?? throw new NotFoundException($"Message {messageId} does not exist.");
        var conversation = await RequireConversation(message.ConversationId);
        conversation.EnsureCanDeleteMessage(userId, message);
        message.Delete(DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
        await _notifier.MessageDeletedAsync(conversation.Id);
    }

    private async Task<Conversation> RequireConversation(Guid id) =>
        await _conversationRepository.GetByIdAsync(id)
        ?? throw new NotFoundException($"Conversation {id} does not exist.");
}

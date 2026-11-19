using Exploration.Application.Conversations;
using Exploration.Application.Tours;
using Payment.Contracts;
using Shared.Domain.Conversations;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourConversationMessaging;

public sealed class TourConversationMessagingService
{
    private const string TourContext = "tour";
    private readonly IConversationRepository _conversationRepository;
    private readonly IChatMessageRepository _messageRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IPaymentApi _paymentApi;
    private readonly IConversationNotifier _notifier;
    private readonly IUnitOfWork _unitOfWork;

    public TourConversationMessagingService(
        IConversationRepository conversationRepository,
        IChatMessageRepository messageRepository,
        ITourRepository tourRepository,
        IPaymentApi paymentApi,
        IConversationNotifier notifier,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _tourRepository = tourRepository;
        _paymentApi = paymentApi;
        _notifier = notifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> OpenTourAsync(Guid userId, Guid tourId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId)
            ?? throw new NotFoundException($"Tour {tourId} does not exist.");
        var hasPurchased = userId != tour.AuthorId && await _paymentApi.HasPurchasedAsync(userId, tourId);
        var role = tour.GetConversationRole(userId, hasPurchased);
        var conversation = await _conversationRepository.GetByContextAsync(TourContext, tourId);

        if (conversation is null)
        {
            conversation = Conversation.CreateContextual(
                ConversationContext.Contextual(TourContext, tourId), [tour.AuthorId]);
            if (userId != tour.AuthorId)
            {
                conversation.AddParticipant(userId, role);
            }
            _conversationRepository.Add(conversation);
            await _unitOfWork.SaveChangesAsync();
        }
        else if (!conversation.CanBeReadBy(userId))
        {
            conversation.AddParticipant(userId, role);
            await _unitOfWork.SaveChangesAsync();
        }

        return conversation.Id;
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

    public async Task MuteAsync(
        Guid userId, Guid conversationId, Guid participantId, MuteDuration duration)
    {
        var conversation = await RequireConversation(conversationId);
        conversation.Mute(userId, participantId, duration, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
        await _notifier.ParticipantMutedAsync(conversationId);
    }

    public async Task UnmuteAsync(Guid userId, Guid conversationId, Guid participantId)
    {
        var conversation = await RequireConversation(conversationId);
        conversation.Unmute(userId, participantId);
        await _unitOfWork.SaveChangesAsync();
        await _notifier.ParticipantUnmutedAsync(conversationId);
    }

    private async Task<Conversation> RequireConversation(Guid id) =>
        await _conversationRepository.GetByIdAsync(id)
        ?? throw new NotFoundException($"Conversation {id} does not exist.");
}

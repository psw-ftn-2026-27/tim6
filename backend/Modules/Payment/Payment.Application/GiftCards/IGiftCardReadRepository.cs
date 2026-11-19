using Shared.Domain;

namespace Payment.Application.GiftCards;

public interface IGiftCardReadRepository
{
    Task<PageResult<GiftCardDto>> GetSentAsync(Guid senderId, DateTime now, int page, int pageSize);
    Task<PageResult<GiftCardDto>> GetReceivedAsync(Guid recipientId, DateTime now, int page, int pageSize);
}

using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardReadRepository : IGiftCardReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public GiftCardReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PageResult<GiftCardDto>> GetSentAsync(Guid senderId, DateTime now, int page, int pageSize)
    {
        return GetPageAsync(
            _dbContext.GiftCards.Where(giftCard => giftCard.SenderId == senderId),
            now,
            page,
            pageSize);
    }

    public Task<PageResult<GiftCardDto>> GetReceivedAsync(Guid recipientId, DateTime now, int page, int pageSize)
    {
        return GetPageAsync(
            _dbContext.GiftCards.Where(giftCard => giftCard.RecipientId == recipientId),
            now,
            page,
            pageSize);
    }

    private static async Task<PageResult<GiftCardDto>> GetPageAsync(
        IQueryable<GiftCard> giftCards,
        DateTime now,
        int page,
        int pageSize)
    {
        var orderedGiftCards = giftCards.OrderByDescending(giftCard => giftCard.CreatedAt);
        var totalCount = await orderedGiftCards.CountAsync();
        var items = await orderedGiftCards
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(giftCard => new GiftCardDto(
                giftCard.Id,
                giftCard.SenderId,
                null,
                giftCard.RecipientId,
                null,
                giftCard.Amount,
                giftCard.Message,
                giftCard.CreatedAt,
                giftCard.ExpiresAt,
                giftCard.IsRedeemed
                    ? GiftCardStatus.Redeemed
                    : now >= giftCard.ExpiresAt
                        ? GiftCardStatus.Expired
                        : GiftCardStatus.Valid))
            .ToListAsync();

        return new PageResult<GiftCardDto>(items, totalCount);
    }
}

using Identity.Contracts;
using Payment.Application.GiftCards;
using Shared.Domain;

namespace Payment.Application.GiftCardBrowsing;

public sealed class GiftCardBrowsingQueries
{
    private readonly IGiftCardReadRepository _giftCardReadRepository;
    private readonly IIdentityApi _identityApi;

    public GiftCardBrowsingQueries(
        IGiftCardReadRepository giftCardReadRepository,
        IIdentityApi identityApi)
    {
        _giftCardReadRepository = giftCardReadRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<GiftCardDto>> GetSentGiftCardsAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var giftCards = await _giftCardReadRepository.GetSentAsync(userId, DateTime.UtcNow, page, pageSize);
        return await AddUserEmailsAsync(giftCards);
    }

    public async Task<PageResult<GiftCardDto>> GetReceivedGiftCardsAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var giftCards = await _giftCardReadRepository.GetReceivedAsync(userId, DateTime.UtcNow, page, pageSize);
        return await AddUserEmailsAsync(giftCards);
    }

    private async Task<PageResult<GiftCardDto>> AddUserEmailsAsync(PageResult<GiftCardDto> giftCards)
    {
        var users = await _identityApi.GetUsersByIdsAsync(
            giftCards.Items.SelectMany(giftCard => new[] { giftCard.SenderId, giftCard.RecipientId }));

        var items = giftCards.Items
            .Select(giftCard => giftCard with
            {
                SenderEmail = users.TryGetValue(giftCard.SenderId, out var sender) ? sender.Email : null,
                RecipientEmail = users.TryGetValue(giftCard.RecipientId, out var recipient) ? recipient.Email : null
            })
            .ToList();

        return giftCards with { Items = items };
    }
}

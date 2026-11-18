using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Purchase;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration.Purchase;

public class PurchaseQueryTests : BaseIntegrationTest
{
    public PurchaseQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_sees_their_purchased_tours_with_name_and_purchase_time_including_a_withdrawn_one_and_without_others_tokens()
    {
        var client = Factory.CreateClientFor(WalletSeed.ReturningBuyer, "explorer");
        var expected = PurchaseTokenSeed.All
            .Where(token => token.UserId == WalletSeed.ReturningBuyer)
            .Select(token => new PurchaseDto(token.Id, token.EntityId, token.Name, token.PurchasedAt));

        var purchases = await client.GetFromJsonAsync<List<PurchaseDto>>("/api/payment/purchases/mine", JsonOptions);

        purchases.Should().BeEquivalentTo(expected);
        purchases.Should().Contain(purchase => purchase.EntityId == ProductSeed.WithdrawnTour.EntityId);
    }
}

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Catalog;

public sealed class PaymentApiTests : BaseIntegrationTest
{
    public PaymentApiTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Putting_a_product_in_offer_stores_the_product_in_offer()
    {
        var entityId = Guid.NewGuid();
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        await paymentApi.PutProductInOfferAsync(entityId, "Petrovaradinska tvrđava", WellKnownUsers.Explorer, 200);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Products.Single(product => product.EntityId == entityId);
        stored.Name.Should().Be("Petrovaradinska tvrđava");
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.Price.Should().Be(200);
        stored.IsInOffer.Should().BeTrue();
    }

    [Fact]
    public async Task Putting_a_product_in_offer_again_updates_the_existing_product()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        await paymentApi.PutProductInOfferAsync(ProductSeed.PaidTourInOffer.EntityId, "Vinogradi i podrumi", WellKnownUsers.Explorer, 350);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Products.Single(product => product.EntityId == ProductSeed.PaidTourInOffer.EntityId);
        stored.Id.Should().Be(ProductSeed.PaidTourInOffer.Id);
        stored.Name.Should().Be("Vinogradi i podrumi");
        stored.Price.Should().Be(350);
        stored.IsInOffer.Should().BeTrue();
    }

    [Fact]
    public async Task Withdrawing_an_existing_product_removes_it_from_the_offer()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        await paymentApi.WithdrawProductFromOfferAsync(ProductSeed.PaidTourInOffer.EntityId);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Single(product => product.Id == ProductSeed.PaidTourInOffer.Id)
            .IsInOffer.Should().BeFalse();
    }

    [Fact]
    public async Task Withdrawing_a_product_that_does_not_exist_is_rejected()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var withdrawal = () => paymentApi.WithdrawProductFromOfferAsync(Guid.NewGuid());

        await withdrawal.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Returning_a_withdrawn_product_puts_it_back_in_the_offer()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        await paymentApi.ReturnProductToOfferAsync(ProductSeed.WithdrawnTour.EntityId);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Products.Single(product => product.Id == ProductSeed.WithdrawnTour.Id);
        stored.IsInOffer.Should().BeTrue();
        stored.Price.Should().Be(ProductSeed.WithdrawnTour.Price);
    }

    [Fact]
    public async Task Price_of_an_existing_product_is_returned()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var price = await paymentApi.GetProductPriceAsync(ProductSeed.PaidTourInOffer.EntityId);

        price.Should().Be(ProductSeed.PaidTourInOffer.Price);
    }

    [Fact]
    public async Task Price_of_a_product_that_does_not_exist_is_rejected()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var priceLookup = () => paymentApi.GetProductPriceAsync(Guid.NewGuid());

        await priceLookup.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Tourist_has_purchased_a_tour_they_bought()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var hasPurchased = await paymentApi.HasPurchasedAsync(WalletSeed.ReturningBuyer, PurchaseTokenSeed.ReturningBuyersFreeTour.EntityId);

        hasPurchased.Should().BeTrue();
    }

    [Fact]
    public async Task Tourist_has_purchased_a_tour_they_bought_even_after_it_was_withdrawn_from_the_offer()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var hasPurchased = await paymentApi.HasPurchasedAsync(WalletSeed.ReturningBuyer, ProductSeed.WithdrawnTour.EntityId);

        hasPurchased.Should().BeTrue();
    }

    [Fact]
    public async Task Tourist_has_not_purchased_a_tour_they_never_bought()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var hasPurchased = await paymentApi.HasPurchasedAsync(WalletSeed.ReturningBuyer, Guid.NewGuid());

        hasPurchased.Should().BeFalse();
    }

    [Fact]
    public async Task Tourist_has_not_purchased_a_tour_that_another_tourist_bought()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var hasPurchased = await paymentApi.HasPurchasedAsync(WalletSeed.ReturningBuyer, PurchaseTokenSeed.DemoExplorersPaidTour.EntityId);

        hasPurchased.Should().BeFalse();
    }
}

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Catalog;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Catalog;

public class CatalogQueryTests : BaseIntegrationTest
{
    public CatalogQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Offer_shows_the_name_and_price_of_products_in_offer_and_not_the_withdrawn_product()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var offer = await client.GetFromJsonAsync<PageResult<ProductDto>>("/api/payment/products/offer", JsonOptions);

        offer!.Items.Should().ContainEquivalentOf(new ProductDto(ProductSeed.FreeTourInOffer.Id, ProductSeed.FreeTourInOffer.Name, ProductSeed.FreeTourInOffer.Price));
        offer.Items.Should().ContainEquivalentOf(new ProductDto(ProductSeed.PaidTourInOffer.Id, ProductSeed.PaidTourInOffer.Name, ProductSeed.PaidTourInOffer.Price));
        offer.Items.Should().NotContain(product => product.Id == ProductSeed.WithdrawnTour.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_the_offer()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/products/offer");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Payment.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringCommandTests : BaseIntegrationTest
{
    public TourAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_creates_a_draft_tour_with_a_recommended_group_size()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"], new RecommendedGroupSizeDto(2, 6));
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
        created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
        created.RecommendedGroupSize.Should().Be(new RecommendedGroupSizeDto(2, 6));
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore + 1);
        var stored = assertContext.Tours.Single(tour => tour.Id == created.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.TransportTimes.Should().BeEmpty();
        stored.RecommendedGroupSize.Should().Be(new RecommendedGroupSize(2, 6));
    }

    [Fact]
    public async Task Explorer_cannot_create_a_tour_without_a_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("   ", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new RecommendedGroupSizeDto(2, 6));
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_tour_whose_recommended_group_size_minimum_exceeds_its_maximum()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new RecommendedGroupSizeDto(7, 6));
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new RecommendedGroupSizeDto(2, 6));

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new RecommendedGroupSizeDto(2, 6));

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_adds_a_transport_time_to_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TransportTimeDto(TransportMode.Walking, 120);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.TransportTimes.Should().ContainSingle(time => time.Transport == TransportMode.Walking && time.Minutes == 120);
    }

    [Fact]
    public async Task Author_publishes_a_paid_tour_gets_back_the_chosen_price_and_the_tour_is_in_the_offer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new PublishTourDto(new TourPriceDto(false, 20));

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var price = await response.Content.ReadFromJsonAsync<TourPriceDto>(JsonOptions);
        price.Should().Be(new TourPriceDto(false, 20));
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.PublishedAt.Should().NotBeNull();
        stored.HasOfferProduct.Should().BeTrue();
        using var paymentContext = Factory.CreateContext<PaymentDbContext>();
        var product = paymentContext.Products.Single(product => product.EntityId == TourSeed.PublishableRiverside.Id);
        product.Name.Should().Be(TourSeed.PublishableRiverside.Name);
        product.Price.Should().Be(20);
        product.IsInOffer.Should().BeTrue();
    }

    [Fact]
    public async Task Tour_with_a_decimal_paid_amount_stays_a_draft_and_is_not_put_in_the_offer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new PublishTourDto(new TourPriceDto(false, 20.5m));

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.PublishedAt.Should().BeNull();
        using var paymentContext = Factory.CreateContext<PaymentDbContext>();
        paymentContext.Products.Should().NotContain(product => product.EntityId == TourSeed.PublishableRiverside.Id);
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        var request = new PublishTourDto(new TourPriceDto(true, null));

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new PublishTourDto(new TourPriceDto(true, null));

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_publish_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new PublishTourDto(new TourPriceDto(true, null));

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_archives_their_published_tour_and_it_is_withdrawn_from_the_offer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Archived);
        stored.ArchivedAt.Should().NotBeNull();
        using var paymentContext = Factory.CreateContext<PaymentDbContext>();
        var product = paymentContext.Products.Single(product => product.EntityId == TourSeed.PublishedVineyards.Id);
        product.IsInOffer.Should().BeFalse();
    }

    [Fact]
    public async Task Published_tour_stays_published_when_payment_rejects_withdrawing_it_from_the_offer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedKovilMissingFromOffer.Id}/archive", null);

        response.IsSuccessStatusCode.Should().BeFalse();
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedKovilMissingFromOffer.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Explorer_cannot_archive_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Author_cannot_archive_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_archive_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_activates_their_archived_tour_and_it_returns_to_the_offer_with_the_same_price()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedSalashes.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedSalashes.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
        stored.PublishedAt.Should().NotBeNull();
        using var paymentContext = Factory.CreateContext<PaymentDbContext>();
        var product = paymentContext.Products.Single(product => product.EntityId == TourSeed.ArchivedSalashes.Id);
        product.IsInOffer.Should().BeTrue();
        product.Price.Should().Be(ProductSeed.WithdrawnSalashes.Price);
    }

    [Fact]
    public async Task Explorer_cannot_activate_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedSalashes.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedSalashes.Id);
        stored.Status.Should().Be(TourStatus.Archived);
        stored.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Author_cannot_activate_a_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedSalashes.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedSalashes.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

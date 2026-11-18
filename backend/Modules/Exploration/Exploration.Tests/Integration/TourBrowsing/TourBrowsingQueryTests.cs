using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingQueryTests : BaseIntegrationTest
{
    public TourBrowsingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Only_published_tours_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.ArchivedSalashes.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Published_tours_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Explorer_who_is_not_the_author_previews_a_published_tour_with_only_its_preview_key_points_in_order()
    {
        var tour = TourSeed.PublishedFortressUndergroundWithPreview;
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var preview = await client.GetFromJsonAsync<TourPreviewDto>($"/api/exploration/tours/{tour.Id}/preview", JsonOptions);

        preview!.Id.Should().Be(tour.Id);
        preview.Name.Should().Be(tour.Name);
        preview.Description.Should().Be(tour.Description);
        preview.Difficulty.Should().Be(tour.Difficulty);
        preview.Tags.Should().Equal(tour.Tags);
        preview.RecommendedGroupSize.Should().Be(new RecommendedGroupSizeDto(tour.RecommendedGroupSize.Min, tour.RecommendedGroupSize.Max));
        preview.LengthInKilometers.Should().Be(tour.Length.Kilometers);
        preview.TransportTimes.Should().BeEquivalentTo(
            tour.TransportTimes.Select(time => new TransportTimeDto(time.Transport, time.Minutes)));
        preview.KeyPoints.Select(keyPoint => keyPoint.Id).Should().Equal(
            tour.KeyPoints
                .Where(keyPoint => keyPoint.IsVisibleWithoutPurchase)
                .OrderBy(keyPoint => keyPoint.Order)
                .Select(keyPoint => keyPoint.Id));
    }

    [Fact]
    public async Task Preview_does_not_reveal_the_secret_of_any_key_point()
    {
        var tour = TourSeed.PublishedFortressUndergroundWithPreview;
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var body = await client.GetStringAsync($"/api/exploration/tours/{tour.Id}/preview");

        foreach (var secret in tour.KeyPoints.Select(keyPoint => keyPoint.Secret).OfType<string>())
        {
            body.Should().NotContain(secret);
        }
    }

    [Fact]
    public async Task Published_tour_without_preview_key_points_is_previewed_with_no_key_points()
    {
        var tour = TourSeed.PublishedPalicWithoutPreview;
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var preview = await client.GetFromJsonAsync<TourPreviewDto>($"/api/exploration/tours/{tour.Id}/preview", JsonOptions);

        preview!.Id.Should().Be(tour.Id);
        preview.LengthInKilometers.Should().Be(tour.Length.Kilometers);
        preview.TransportTimes.Should().NotBeEmpty();
        preview.KeyPoints.Should().BeEmpty();
    }

    public static TheoryData<Guid> ToursThatAreNotPublished => new()
    {
        TourSeed.PublishableRiverside.Id,
        TourSeed.ArchivedSalashes.Id,
        Guid.Parse("7c1e2a4b-0000-4000-8000-000000000404")
    };

    [Theory]
    [MemberData(nameof(ToursThatAreNotPublished))]
    public async Task Tour_that_is_not_published_cannot_be_previewed(Guid tourId)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{tourId}/preview");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

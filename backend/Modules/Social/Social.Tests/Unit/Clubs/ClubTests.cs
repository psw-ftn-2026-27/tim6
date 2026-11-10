using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static Club CreateClub() =>
        new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend šetnje stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);

    [Theory]
    [InlineData("", "Vikend šetnje.")]
    [InlineData("   ", "Vikend šetnje.")]
    [InlineData("Planinari", "")]
    [InlineData("Planinari", "   ")]
    public void New_club_requires_a_name_and_a_description(string name, string description)
    {
        var creation = () => new Club(WellKnownUsers.Explorer, name, description, ["https://example.com/slika.jpg"]);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_club_requires_at_least_one_image()
    {
        var creation = () => new Club(WellKnownUsers.Explorer, "Planinari", "Vikend šetnje.", []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Vikend šetnje.")]
    [InlineData("   ", "Vikend šetnje.")]
    [InlineData("Planinari", "")]
    [InlineData("Planinari", "   ")]
    public void Updated_club_keeps_a_name_and_a_description(string name, string description)
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.Explorer, name, description, ["https://example.com/slika.jpg"]);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Updated_club_keeps_at_least_one_image()
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.Explorer, "Planinari", "Vikend šetnje.", []);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_update_the_club()
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.SecondExplorer, "Tuđa izmena", "Opis tuđe izmene.", ["https://example.com/slika.jpg"]);

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_delete_the_club()
    {
        var club = CreateClub();

        var deletion = () => club.Delete(WellKnownUsers.SecondExplorer);

        deletion.Should().Throw<NotFoundException>();
    }
}

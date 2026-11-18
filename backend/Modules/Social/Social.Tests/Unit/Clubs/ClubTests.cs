using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static Club CreateClub() =>
        new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend šetnje stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"], DateTime.UtcNow);

    private static List<string> CreateImages(int count) =>
        Enumerable.Range(1, count).Select(number => $"/api/shared/images/slika-{number}.jpg").ToList();

    [Theory]
    [InlineData("", "Vikend šetnje.")]
    [InlineData("   ", "Vikend šetnje.")]
    [InlineData("Planinari", "")]
    [InlineData("Planinari", "   ")]
    public void New_club_requires_a_name_and_a_description(string name, string description)
    {
        var creation = () => new Club(WellKnownUsers.Explorer, name, description, ["https://example.com/slika.jpg"], DateTime.UtcNow);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_club_requires_at_least_one_image()
    {
        var creation = () => new Club(WellKnownUsers.Explorer, "Planinari", "Vikend šetnje.", [], DateTime.UtcNow);

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
    public void New_club_may_have_five_images()
    {
        var club = new Club(WellKnownUsers.Explorer, "Planinari", "Vikend šetnje.", CreateImages(5), DateTime.UtcNow);

        club.Images.Should().HaveCount(5);
    }

    [Fact]
    public void New_club_has_at_most_five_images()
    {
        var creation = () => new Club(WellKnownUsers.Explorer, "Planinari", "Vikend šetnje.", CreateImages(6), DateTime.UtcNow);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Updated_club_keeps_at_most_five_images()
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.Explorer, "Planinari", "Vikend šetnje.", CreateImages(6));

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

    [Fact]
    public void Founder_cannot_request_to_join_their_own_club()
    {
        var club = CreateClub();

        var request = () => club.RequestToJoin(WellKnownUsers.Explorer);

        request.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_with_a_pending_request_cannot_send_another_request_to_the_same_club()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        var request = () => club.RequestToJoin(WellKnownUsers.SecondExplorer);

        request.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_withdraw_another_users_request_and_that_request_remains()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        var withdrawal = () => club.WithdrawJoinRequest(WellKnownUsers.Administrator);

        withdrawal.Should().Throw<DomainException>();
        club.JoinRequests.Should().ContainSingle(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void Plain_member_cannot_accept_a_join_request_and_the_request_remains()
    {
        var club = CreateClubWithAPlainMemberAndAPendingRequest();

        var acceptance = () => club.AcceptJoinRequest(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

        acceptance.Should().Throw<DomainException>();
        club.JoinRequests.Should().ContainSingle(request => request.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public void Plain_member_cannot_reject_a_join_request_and_the_request_remains()
    {
        var club = CreateClubWithAPlainMemberAndAPendingRequest();

        var rejection = () => club.RejectJoinRequest(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

        rejection.Should().Throw<DomainException>();
        club.JoinRequests.Should().ContainSingle(request => request.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public void User_without_a_pending_request_cannot_be_accepted_and_does_not_become_a_member()
    {
        var club = CreateClub();

        var acceptance = () => club.AcceptJoinRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        acceptance.Should().Throw<DomainException>();
        club.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
    }

    private static Club CreateClubWithAPlainMemberAndAPendingRequest()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);
        club.AcceptJoinRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        club.RequestToJoin(WellKnownUsers.Administrator);
        return club;
    }
}

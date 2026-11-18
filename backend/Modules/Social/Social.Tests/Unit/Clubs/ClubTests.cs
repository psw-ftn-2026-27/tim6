using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static readonly Guid FirstPlainMember = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private static readonly Guid SecondPlainMember = Guid.Parse("40000000-0000-0000-0000-000000000002");

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

    [Fact]
    public void Moderator_cannot_promote_a_member_and_the_member_stays_plain()
    {
        var club = CreateClubWithTwoModeratorsAndTwoPlainMembers();

        var promotion = () => club.PromoteToModerator(WellKnownUsers.SecondExplorer, FirstPlainMember);

        promotion.Should().Throw<DomainException>();
        club.Members.Should().ContainSingle(member => member.UserId == FirstPlainMember && member.Role == ClubRole.Member);
    }

    [Fact]
    public void Moderator_cannot_be_promoted_again()
    {
        var club = CreateClubWithTwoModeratorsAndTwoPlainMembers();

        var promotion = () => club.PromoteToModerator(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        promotion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Owner_cannot_remove_themselves_and_remains_the_owner()
    {
        var club = CreateClubWithTwoModeratorsAndTwoPlainMembers();

        var removal = () => club.RemoveMember(WellKnownUsers.Explorer, WellKnownUsers.Explorer);

        removal.Should().Throw<DomainException>();
        club.Members.Should().ContainSingle(member => member.UserId == WellKnownUsers.Explorer && member.Role == ClubRole.Owner);
    }

    [Fact]
    public void Owner_can_remove_a_moderator()
    {
        var club = CreateClubWithTwoModeratorsAndTwoPlainMembers();

        club.RemoveMember(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void Moderator_cannot_remove_another_moderator_and_that_moderator_remains()
    {
        var club = CreateClubWithTwoModeratorsAndTwoPlainMembers();

        var removal = () => club.RemoveMember(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

        removal.Should().Throw<DomainException>();
        club.Members.Should().ContainSingle(member => member.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public void Plain_member_cannot_remove_another_member_and_that_member_remains()
    {
        var club = CreateClubWithTwoModeratorsAndTwoPlainMembers();

        var removal = () => club.RemoveMember(FirstPlainMember, SecondPlainMember);

        removal.Should().Throw<DomainException>();
        club.Members.Should().ContainSingle(member => member.UserId == SecondPlainMember);
    }

    [Fact]
    public void Moderator_can_accept_a_join_request()
    {
        var club = CreateClubWithTwoModeratorsAndTwoPlainMembers();
        var applicant = Guid.NewGuid();
        club.RequestToJoin(applicant);

        club.AcceptJoinRequest(WellKnownUsers.SecondExplorer, applicant);

        club.Members.Should().ContainSingle(member => member.UserId == applicant && member.Role == ClubRole.Member);
    }

    private static Club CreateClubWithTwoModeratorsAndTwoPlainMembers()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);
        club.AcceptJoinRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        club.PromoteToModerator(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        club.RequestToJoin(WellKnownUsers.Administrator);
        club.AcceptJoinRequest(WellKnownUsers.Explorer, WellKnownUsers.Administrator);
        club.PromoteToModerator(WellKnownUsers.Explorer, WellKnownUsers.Administrator);
        club.RequestToJoin(FirstPlainMember);
        club.AcceptJoinRequest(WellKnownUsers.Explorer, FirstPlainMember);
        club.RequestToJoin(SecondPlainMember);
        club.AcceptJoinRequest(WellKnownUsers.Explorer, SecondPlainMember);
        return club;
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

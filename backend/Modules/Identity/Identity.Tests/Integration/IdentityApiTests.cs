using FluentAssertions;
using Identity.Contracts;
using Identity.Tests.Integration.Seeds;
using Microsoft.Extensions.DependencyInjection;
using Shared.Tests;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityApiTests : BaseIntegrationTest
{
    public IdentityApiTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task existing_user_is_returned_by_id_with_email()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await identityApi.GetUserByIdAsync(WellKnownUsers.Explorer);

        result.Should().Be(new UserSummaryDto(WellKnownUsers.Explorer, UserSeed.Explorer.Email));
    }

    [Fact]
    public async Task missing_user_by_id_returns_null()
    {
        var missingUserId = Guid.NewGuid();
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await identityApi.GetUserByIdAsync(missingUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task group_lookup_by_ids_returns_each_found_user_once()
    {
        var missingUserId = Guid.NewGuid();
        var userIds = new[] { WellKnownUsers.Explorer, missingUserId, WellKnownUsers.Explorer };
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var results = await identityApi.GetUsersByIdsAsync(userIds);

        results.Should().BeEquivalentTo(new Dictionary<Guid, UserSummaryDto>
        {
            [WellKnownUsers.Explorer] = new(WellKnownUsers.Explorer, UserSeed.Explorer.Email)
        });
    }

    [Fact]
    public async Task existing_user_is_returned_by_email_ignoring_letter_case()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await identityApi.GetUserByEmailAsync(ExplorerEmail.ToUpperInvariant());

        result.Should().Be(new UserSummaryDto(WellKnownUsers.Explorer, UserSeed.Explorer.Email));
    }

    [Theory]
    [InlineData("explorer@test")]
    [InlineData("missing@test.com")]
    public async Task partial_or_missing_email_returns_null(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await identityApi.GetUserByEmailAsync(email);

        result.Should().BeNull();
    }

    [Fact]
    public async Task group_lookup_by_emails_returns_each_found_user_once()
    {
        var email = ExplorerEmail;
        var emails = new[] { email.ToUpperInvariant(), "missing@test.com", email };
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var results = await identityApi.GetUsersByEmailsAsync(emails);

        results.Should().BeEquivalentTo(new Dictionary<string, UserSummaryDto>
        {
            [email] = new(WellKnownUsers.Explorer, email)
        });
    }

    [Fact]
    public async Task partial_email_search_returns_a_stable_page_and_total_count()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await identityApi.SearchUsersAsync("EXPLORER", 1, 1);

        result.Items.Should().ContainSingle();
        result.Items[0].Should().Be(new UserSummaryDto(WellKnownUsers.Explorer, ExplorerEmail));
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task missing_email_search_returns_an_empty_page()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await identityApi.SearchUsersAsync("nobody-with-this-email", 1, 20);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    private static string ExplorerEmail =>
        UserSeed.Explorer.Email ?? throw new InvalidOperationException("The explorer seed must have an email.");
}

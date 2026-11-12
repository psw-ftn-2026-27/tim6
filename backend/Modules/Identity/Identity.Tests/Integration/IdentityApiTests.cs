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
    public async Task Existing_user_is_returned_with_email()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await identityApi.GetUserAsync(WellKnownUsers.Explorer);

        result.Should().Be(new UserSummaryDto(WellKnownUsers.Explorer, UserSeed.Explorer.Email, true));
    }

    [Fact]
    public async Task Missing_user_is_returned_without_email()
    {
        var missingUserId = Guid.NewGuid();
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var result = await identityApi.GetUserAsync(missingUserId);

        result.Should().Be(new UserSummaryDto(missingUserId, null, false));
    }

    [Fact]
    public async Task Group_lookup_keeps_found_and_missing_users_distinct()
    {
        var missingUserId = Guid.NewGuid();
        var userIds = new[] { WellKnownUsers.Explorer, missingUserId, WellKnownUsers.Explorer };
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var results = await identityApi.GetUsersAsync(userIds);

        results.Should().BeEquivalentTo(
        [
            new UserSummaryDto(WellKnownUsers.Explorer, UserSeed.Explorer.Email, true),
            new UserSummaryDto(missingUserId, null, false)
        ], options => options.WithStrictOrdering());
    }
}

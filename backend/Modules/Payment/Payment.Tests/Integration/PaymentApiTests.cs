using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration;

public sealed class PaymentApiTests : BaseIntegrationTest
{
    public PaymentApiTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Every_tour_counts_as_purchased_until_tour_purchase_is_implemented()
    {
        var tourId = Guid.NewGuid();
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var result = await paymentApi.HasPurchasedTourAsync(WellKnownUsers.Explorer, tourId);

        result.Should().BeTrue();
    }
}

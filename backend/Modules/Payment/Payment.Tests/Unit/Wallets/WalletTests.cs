using FluentAssertions;
using Payment.Domain.Wallets;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Wallets;

public class WalletTests
{
    [Fact]
    public void New_wallet_has_a_balance_of_zero()
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);

        var balance = wallet.Balance();

        balance.Should().Be(0);
    }
}

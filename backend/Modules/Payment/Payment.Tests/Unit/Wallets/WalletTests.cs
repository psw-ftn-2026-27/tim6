using FluentAssertions;
using Payment.Domain.Carts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Wallets;

public class WalletTests
{
    private static readonly DateTime DepositedAt = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void New_wallet_has_a_balance_of_zero()
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);

        var balance = wallet.Balance();

        balance.Should().Be(0);
    }

    [Fact]
    public void Balance_is_the_sum_of_deposits_without_the_cancelled_ones_less_the_sum_of_payments()
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);
        wallet.Deposit(WellKnownUsers.Administrator, 300, DepositedAt);
        wallet.Deposit(WellKnownUsers.Administrator, 200, DepositedAt);
        wallet.Deposit(WellKnownUsers.Administrator, 50, DepositedAt);
        wallet.CancelTransaction(wallet.Transactions[1].Id, "Pogrešan iznos.", DepositedAt.AddHours(1));
        var item = new CartItem(Guid.NewGuid(), "Bački salaši", 150);
        wallet.Pay([item], new Dictionary<Guid, Guid> { [item.EntityId] = WellKnownUsers.SecondExplorer }, DepositedAt.AddHours(2));

        var balance = wallet.Balance();

        balance.Should().Be(200);
    }

    [Fact]
    public void Payment_greater_than_the_balance_is_rejected_and_no_payment_is_added()
    {
        var wallet = CreateWalletWithOneDeposit();
        var cheapItem = new CartItem(Guid.NewGuid(), "Šetnja tvrđavom", 40);
        var expensiveItem = new CartItem(Guid.NewGuid(), "Vinogradi Sremskih Karlovaca", 61);
        var ownerIds = new Dictionary<Guid, Guid>
        {
            [cheapItem.EntityId] = WellKnownUsers.SecondExplorer,
            [expensiveItem.EntityId] = WellKnownUsers.SecondExplorer
        };

        var payment = () => wallet.Pay([cheapItem, expensiveItem], ownerIds, DepositedAt.AddHours(1));

        payment.Should().Throw<DomainException>();
        wallet.Payments.Should().BeEmpty();
    }

    [Fact]
    public void Payment_equal_to_the_balance_adds_one_payment_for_each_item()
    {
        var wallet = CreateWalletWithOneDeposit();
        var freeItem = new CartItem(Guid.NewGuid(), "Šetnja tvrđavom", 0);
        var paidItem = new CartItem(Guid.NewGuid(), "Vinogradi Sremskih Karlovaca", 100);
        var ownerIds = new Dictionary<Guid, Guid>
        {
            [freeItem.EntityId] = WellKnownUsers.Explorer,
            [paidItem.EntityId] = WellKnownUsers.SecondExplorer
        };
        var paidAt = DepositedAt.AddHours(1);

        wallet.Pay([freeItem, paidItem], ownerIds, paidAt);

        wallet.Payments.Should().HaveCount(2);
        wallet.Payments.Should().ContainSingle(payment =>
            payment.EntityId == freeItem.EntityId && payment.Name == freeItem.Name
            && payment.OwnerId == WellKnownUsers.Explorer && payment.Price == 0 && payment.PaidAt == paidAt);
        wallet.Payments.Should().ContainSingle(payment =>
            payment.EntityId == paidItem.EntityId && payment.Name == paidItem.Name
            && payment.OwnerId == WellKnownUsers.SecondExplorer && payment.Price == 100 && payment.PaidAt == paidAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Deposit_of_less_than_one_is_rejected(int amount)
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);

        var deposit = () => wallet.Deposit(WellKnownUsers.Administrator, amount, DepositedAt);

        deposit.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancellation_exactly_24_hours_after_the_deposit_records_the_mark_time_and_reason()
    {
        var wallet = CreateWalletWithOneDeposit();
        var deposit = wallet.Transactions.Single();
        var cancelledAt = DepositedAt.AddHours(24);

        wallet.CancelTransaction(deposit.Id, "Uplata greškom.", cancelledAt);

        deposit.IsCancelled.Should().BeTrue();
        deposit.CancelledAt.Should().Be(cancelledAt);
        deposit.CancellationReason.Should().Be("Uplata greškom.");
    }

    [Fact]
    public void Deposit_older_than_24_hours_cannot_be_cancelled()
    {
        var wallet = CreateWalletWithOneDeposit();
        var deposit = wallet.Transactions.Single();

        var cancellation = () => wallet.CancelTransaction(deposit.Id, "Uplata greškom.", DepositedAt.AddHours(24).AddSeconds(1));

        cancellation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancellation_without_a_reason_is_rejected(string reason)
    {
        var wallet = CreateWalletWithOneDeposit();
        var deposit = wallet.Transactions.Single();

        var cancellation = () => wallet.CancelTransaction(deposit.Id, reason, DepositedAt.AddHours(1));

        cancellation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deposit_that_is_already_cancelled_cannot_be_cancelled_again()
    {
        var wallet = CreateWalletWithOneDeposit();
        var deposit = wallet.Transactions.Single();
        wallet.CancelTransaction(deposit.Id, "Uplata greškom.", DepositedAt.AddHours(1));

        var cancellation = () => wallet.CancelTransaction(deposit.Id, "Ponovo.", DepositedAt.AddHours(2));

        cancellation.Should().Throw<DomainException>();
    }

    private static Wallet CreateWalletWithOneDeposit()
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);
        wallet.Deposit(WellKnownUsers.Administrator, 100, DepositedAt);
        return wallet;
    }
}

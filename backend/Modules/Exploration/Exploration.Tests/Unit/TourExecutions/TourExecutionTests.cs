using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourExecutionTests
{
    private static TourExecution CreateActiveExecution() =>
        new(WellKnownUsers.Explorer, Guid.NewGuid(), new Location(45.2530, 19.8620));

    [Fact]
    public void Abandoned_session_is_abandoned_with_an_end_time()
    {
        var execution = CreateActiveExecution();

        execution.Abandon();

        execution.Status.Should().Be(TourExecutionStatus.Abandoned);
        execution.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Session_that_is_not_active_cannot_be_abandoned()
    {
        var execution = CreateActiveExecution();
        execution.Abandon();

        var abandonment = () => execution.Abandon();

        abandonment.Should().Throw<DomainException>();
    }
}

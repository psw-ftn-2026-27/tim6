using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourLengthTests
{
    [Fact]
    public void Negative_tour_length_is_rejected()
    {
        var creation = () => new TourLength(-0.1);

        creation.Should().Throw<DomainException>();
    }
}

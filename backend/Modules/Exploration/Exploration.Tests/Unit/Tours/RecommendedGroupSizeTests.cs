using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class RecommendedGroupSizeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Recommended_group_size_starts_at_one_person(int min)
    {
        var creation = () => new RecommendedGroupSize(min, 6);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Recommended_group_size_minimum_cannot_exceed_its_maximum()
    {
        var creation = () => new RecommendedGroupSize(7, 6);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Recommended_group_size_may_be_a_single_number_of_people()
    {
        var groupSize = new RecommendedGroupSize(4, 4);

        groupSize.Min.Should().Be(4);
        groupSize.Max.Should().Be(4);
    }
}

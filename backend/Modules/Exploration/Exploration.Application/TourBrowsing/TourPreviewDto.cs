using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourPreviewDto(
    Guid Id,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    RecommendedGroupSizeDto RecommendedGroupSize,
    double LengthInKilometers,
    List<TransportTimeDto> TransportTimes,
    List<KeyPointPreviewDto> KeyPoints);

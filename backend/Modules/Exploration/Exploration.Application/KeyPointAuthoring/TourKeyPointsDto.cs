using Exploration.Domain.Tours;

namespace Exploration.Application.KeyPointAuthoring;

public sealed record TourKeyPointsDto(TourStatus Status, List<KeyPointDto> KeyPoints, double LengthInKilometers);

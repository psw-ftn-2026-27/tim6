using Exploration.Application.Tours;

namespace Exploration.Application.TourExecuting;

public sealed record UnlockedKeyPointDto(string Name, LocationDto Location, string? Secret);

using Exploration.Application.Tours;

namespace Exploration.Application.TourExecuting;

public sealed record NextKeyPointDto(string Name, string Description, string Image, LocationDto Location);

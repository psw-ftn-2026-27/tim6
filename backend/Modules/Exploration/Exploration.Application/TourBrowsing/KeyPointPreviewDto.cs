using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record KeyPointPreviewDto(
    Guid Id,
    string Name,
    string Description,
    string Image,
    LocationDto Location);

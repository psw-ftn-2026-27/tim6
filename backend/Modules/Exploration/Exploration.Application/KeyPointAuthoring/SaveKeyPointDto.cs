namespace Exploration.Application.KeyPointAuthoring;

public sealed record SaveKeyPointDto(
    string Name,
    string Description,
    string Image,
    string? Secret,
    LocationDto Location,
    bool IsVisibleWithoutPurchase,
    double LengthInKilometers);

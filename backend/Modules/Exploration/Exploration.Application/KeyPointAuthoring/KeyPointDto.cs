using Exploration.Application.Tours;

namespace Exploration.Application.KeyPointAuthoring;

public sealed record KeyPointDto(
    Guid Id,
    int Order,
    string Name,
    string Description,
    string Image,
    string? Secret,
    LocationDto Location,
    bool IsVisibleWithoutPurchase);

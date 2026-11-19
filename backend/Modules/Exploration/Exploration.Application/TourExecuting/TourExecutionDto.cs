using Exploration.Domain.TourExecutions;

namespace Exploration.Application.TourExecuting;

public sealed record TourExecutionDto(
    Guid Id,
    Guid TourId,
    TourExecutionStatus Status,
    NextKeyPointDto? NextKeyPoint,
    UnlockedKeyPointDto? LastReachedKeyPoint);

namespace Exploration.Application.TourExecuting;

public sealed record TourExecutionDto(Guid Id, Guid TourId, NextKeyPointDto NextKeyPoint);

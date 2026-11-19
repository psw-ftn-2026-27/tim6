using Exploration.Application.Tours;

namespace Exploration.Application.TourExecuting;

public sealed record StartTourDto(Guid TourId, LocationDto Position);

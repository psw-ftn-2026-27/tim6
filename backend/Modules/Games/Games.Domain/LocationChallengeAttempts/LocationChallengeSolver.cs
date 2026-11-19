using Games.Domain.Challenges;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeAttempts;

// Activation and location checks need both the challenge and the explorer's attempt,
// so the rules that span the two aggregates live here.
public sealed class LocationChallengeSolver
{
    public const double MaxExtraMeters = 100;
    private const double MaxRevealDistanceInMeters = 5;

    public LocationChallengeAttempt Activate(LocationChallenge challenge, LocationChallengeAttempt? existingAttempt,
        Guid explorerId, Location position, double extraMeters)
    {
        EnsurePublished(challenge);
        if (existingAttempt is not null && existingAttempt.CompletedAt is not null)
        {
            throw new DomainException("You have already solved this location challenge.");
        }
        if (existingAttempt is not null)
        {
            return existingAttempt;
        }

        // Only a new attempt checks proximity; an open one is continued from anywhere.
        challenge.EnsureCanBeActivatedFrom(position);
        if (extraMeters < 0 || extraMeters > MaxExtraMeters)
        {
            throw new DomainException($"The extra radius is between 0 and {MaxExtraMeters} m.");
        }

        // A published challenge always has a start location.
        var start = challenge.Location!;
        var farthestPointDistance = challenge.HiddenPoints.Max(point => start.DistanceTo(point.Location));
        return new LocationChallengeAttempt(challenge.Id, explorerId, farthestPointDistance + extraMeters);
    }

    public void CheckLocation(LocationChallenge challenge, LocationChallengeAttempt attempt, Location position)
    {
        EnsurePublished(challenge);

        var foundPoint = challenge.HiddenPoints.FirstOrDefault(point =>
            !attempt.RevealedHiddenPointIds.Contains(point.Id)
            && point.Location.DistanceTo(position) <= MaxRevealDistanceInMeters);
        if (foundPoint is null)
        {
            throw new DomainException("You are not at a hidden location.");
        }

        attempt.Reveal(foundPoint.Id);
        if (challenge.HiddenPoints.All(point => attempt.RevealedHiddenPointIds.Contains(point.Id)))
        {
            attempt.Complete();
        }
    }

    private static void EnsurePublished(LocationChallenge challenge)
    {
        if (challenge.Status != ChallengeStatus.Published)
        {
            throw new DomainException("This location challenge is not available.");
        }
    }
}

using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeAttempts;

// One explorer's attempt at one location challenge. It refers to both only by ID and
// keeps the circle and the revealed hidden points until the challenge is completed.
public sealed class LocationChallengeAttempt : AggregateRoot
{
    private readonly List<Guid> _revealedHiddenPointIds = [];

    public Guid LocationChallengeId { get; private set; }
    public Guid ExplorerId { get; private set; }
    public double RadiusInMeters { get; private set; }
    public DateTime ActivatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? EarnedXp { get; private set; }
    public IReadOnlyList<Guid> RevealedHiddenPointIds => _revealedHiddenPointIds;

    private LocationChallengeAttempt() { }

    public LocationChallengeAttempt(Guid locationChallengeId, Guid explorerId, double radiusInMeters)
        : base(Guid.NewGuid())
    {
        LocationChallengeId = locationChallengeId;
        ExplorerId = explorerId;
        RadiusInMeters = radiusInMeters;
        ActivatedAt = DateTime.UtcNow;
    }

    public void Reveal(Guid hiddenPointId)
    {
        if (CompletedAt is not null)
        {
            throw new DomainException("This location challenge is already solved.");
        }
        if (_revealedHiddenPointIds.Contains(hiddenPointId))
        {
            throw new DomainException("This hidden point is already revealed.");
        }

        _revealedHiddenPointIds.Add(hiddenPointId);
    }

    public void Complete(int xp)
    {
        if (CompletedAt is not null)
        {
            throw new DomainException("This location challenge is already solved.");
        }

        CompletedAt = DateTime.UtcNow;
        EarnedXp = xp;
    }
}

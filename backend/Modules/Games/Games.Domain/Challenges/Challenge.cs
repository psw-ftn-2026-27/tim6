using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Challenges;

// The lifecycle shared by every kind of challenge: a location set while drafting and the
// moves between draft, published, retired and archived. Each kind adds its own data,
// validation and editing.
public abstract class Challenge : AggregateRoot
{
    public Location? Location { get; private set; }
    public ChallengeStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Names the kind in error messages, for example "quiz" or "social challenge".
    protected abstract string KindName { get; }

    // How close a tourist must be to activate the challenge.
    public abstract int ActivationRadiusInMeters { get; }

    protected Challenge() { }

    protected Challenge(Guid id)
        : base(id)
    {
        Status = ChallengeStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void PlaceAt(Location location)
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException($"Only a draft {KindName} can be placed.");
        }

        Location = location;
    }

    public void RemoveLocation()
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException($"Only a draft {KindName} can have its location removed.");
        }

        Location = null;
    }

    public void Publish()
    {
        if (Status != ChallengeStatus.Draft && Status != ChallengeStatus.Retired)
        {
            throw new DomainException($"Only a draft or retired {KindName} can be published.");
        }
        if (Location is null)
        {
            throw new DomainException($"A {KindName} can be published only with a location.");
        }

        Status = ChallengeStatus.Published;
    }

    public void Retire()
    {
        if (Status != ChallengeStatus.Published)
        {
            throw new DomainException($"Only a published {KindName} can be retired.");
        }

        Status = ChallengeStatus.Retired;
    }

    public void Archive()
    {
        if (Status != ChallengeStatus.Retired)
        {
            throw new DomainException($"Only a retired {KindName} can be archived.");
        }

        Status = ChallengeStatus.Archived;
    }

    // A tourist activates only a published challenge, and only from within its activation radius.
    public void EnsureCanBeActivatedFrom(Location position)
    {
        EnsurePublished();
        if (!IsWithinRadius(Location!, ActivationRadiusInMeters, position))
        {
            throw new DomainException($"A {KindName} can be activated only within {ActivationRadiusInMeters} m of it.");
        }
    }

    // Whether a tourist at the position is close enough to a challenge at the location; a tourist
    // without a position is not. Static so that a read use case can apply it without loading
    // the challenge.
    public static bool IsWithinRadius(Location challengeLocation, int radiusInMeters, Location? position)
    {
        return position is not null && challengeLocation.DistanceTo(position) <= radiusInMeters;
    }

    // A published challenge always has a location, because publishing requires one.
    protected void EnsurePublished()
    {
        if (Status != ChallengeStatus.Published || Location is null)
        {
            throw new DomainException($"Only a published {KindName} can be played.");
        }
    }

    public void Delete()
    {
        if (Status != ChallengeStatus.Draft && Status != ChallengeStatus.Archived)
        {
            throw new DomainException($"Only a draft or archived {KindName} can be deleted.");
        }
    }
}

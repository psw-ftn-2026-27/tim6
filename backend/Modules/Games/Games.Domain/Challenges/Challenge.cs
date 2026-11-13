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

    public void Delete()
    {
        if (Status != ChallengeStatus.Draft && Status != ChallengeStatus.Archived)
        {
            throw new DomainException($"Only a draft or archived {KindName} can be deleted.");
        }
    }
}

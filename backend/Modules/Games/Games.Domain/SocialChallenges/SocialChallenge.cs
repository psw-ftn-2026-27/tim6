using Games.Domain.Challenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed class SocialChallenge : AggregateRoot
{
    private const int MaxNameLength = 200;
    private const int MinRequiredPeople = 2;
    private const int MaxRequiredPeople = 30;
    private const int MinRangeInMeters = 20;
    private const int MaxRangeInMeters = 300;

    public string Name { get; private set; }
    public string ExitMessage { get; private set; }
    public int RequiredPeople { get; private set; }
    public int RangeInMeters { get; private set; }
    public int Xp { get; private set; }
    public DailyTimeWindow TimeWindow { get; private set; }
    public Location? Location { get; private set; }
    public ChallengeStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private SocialChallenge()
    {
        Name = null!;
        ExitMessage = null!;
        TimeWindow = null!;
    }

    public SocialChallenge(string name, DailyTimeWindow timeWindow, int requiredPeople, int rangeInMeters, int xp,
        string exitMessage)
        : base(Guid.NewGuid())
    {
        Validate(name, timeWindow, requiredPeople, rangeInMeters, xp, exitMessage);

        Name = name;
        TimeWindow = timeWindow;
        RequiredPeople = requiredPeople;
        RangeInMeters = rangeInMeters;
        Xp = xp;
        ExitMessage = exitMessage;
        Status = ChallengeStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void Edit(string name, DailyTimeWindow timeWindow, int requiredPeople, int rangeInMeters, int xp,
        string exitMessage)
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft social challenge can be edited.");
        }
        Validate(name, timeWindow, requiredPeople, rangeInMeters, xp, exitMessage);

        Name = name;
        TimeWindow = timeWindow;
        RequiredPeople = requiredPeople;
        RangeInMeters = rangeInMeters;
        Xp = xp;
        ExitMessage = exitMessage;
    }

    public void PlaceAt(Location location)
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft social challenge can be placed.");
        }

        Location = location;
    }

    public void RemoveLocation()
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft social challenge can have its location removed.");
        }

        Location = null;
    }

    public void Publish()
    {
        if (Status != ChallengeStatus.Draft && Status != ChallengeStatus.Retired)
        {
            throw new DomainException("Only a draft or retired social challenge can be published.");
        }
        if (Location is null)
        {
            throw new DomainException("A social challenge can be published only with a location.");
        }

        Status = ChallengeStatus.Published;
    }

    public void Retire()
    {
        if (Status != ChallengeStatus.Published)
        {
            throw new DomainException("Only a published social challenge can be retired.");
        }

        Status = ChallengeStatus.Retired;
    }

    public void Archive()
    {
        if (Status != ChallengeStatus.Retired)
        {
            throw new DomainException("Only a retired social challenge can be archived.");
        }

        Status = ChallengeStatus.Archived;
    }

    public void Delete()
    {
        if (Status != ChallengeStatus.Draft && Status != ChallengeStatus.Archived)
        {
            throw new DomainException("Only a draft or archived social challenge can be deleted.");
        }
    }

    private static void Validate(string name, DailyTimeWindow timeWindow, int requiredPeople, int rangeInMeters,
        int xp, string exitMessage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A social challenge requires a name.");
        }
        if (name.Length > MaxNameLength)
        {
            throw new DomainException($"A social challenge name has at most {MaxNameLength} characters.");
        }
        if (string.IsNullOrWhiteSpace(exitMessage))
        {
            throw new DomainException("A social challenge requires an exit message.");
        }
        if (timeWindow is null)
        {
            throw new DomainException("A social challenge requires a daily time window.");
        }
        if (requiredPeople < MinRequiredPeople || requiredPeople > MaxRequiredPeople)
        {
            throw new DomainException($"A social challenge requires between {MinRequiredPeople} and {MaxRequiredPeople} people.");
        }
        if (rangeInMeters < MinRangeInMeters || rangeInMeters > MaxRangeInMeters)
        {
            throw new DomainException($"A social challenge range is between {MinRangeInMeters} and {MaxRangeInMeters} meters.");
        }
        if (xp <= 0)
        {
            throw new DomainException("A social challenge rewards a positive amount of XP.");
        }
    }
}

using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed class SocialChallenge : AggregateRoot
{
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
        EnsureDraft();
        Validate(name, timeWindow, requiredPeople, rangeInMeters, xp, exitMessage);

        Name = name;
        TimeWindow = timeWindow;
        RequiredPeople = requiredPeople;
        RangeInMeters = rangeInMeters;
        Xp = xp;
        ExitMessage = exitMessage;
    }

    public void Delete()
    {
        EnsureDraft();
    }

    private void EnsureDraft()
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft social challenge can be changed or deleted.");
        }
    }

    private static void Validate(string name, DailyTimeWindow timeWindow, int requiredPeople, int rangeInMeters,
        int xp, string exitMessage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A social challenge requires a name.");
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

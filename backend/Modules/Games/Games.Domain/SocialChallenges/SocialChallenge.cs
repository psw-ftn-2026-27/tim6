using Games.Domain.Challenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed class SocialChallenge : Challenge
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
    public SocialChallengeRun? ActiveRun { get; private set; }

    protected override string KindName => "social challenge";
    public override int ActivationRadiusInMeters => RangeInMeters;

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

    // Whether a tourist at the position can activate a published social challenge at the moment,
    // for a read use case that shows social challenges without loading them.
    public static bool CanBeActivated(Location location, int rangeInMeters, DailyTimeWindow timeWindow,
        Location? position, bool isSolved, DateTime moment)
    {
        return !isSolved && IsWithinRadius(location, rangeInMeters, position) && timeWindow.IsOpenAt(moment);
    }

    // Every moment is in UTC. A tourist joins the active run, or starts a new one when there is
    // none or the window of the last one has ended.
    public void Activate(Guid touristId, Location position, DateTime moment)
    {
        EnsureCanBeActivatedFrom(position);
        if (!TimeWindow.IsOpenAt(moment))
        {
            throw new DomainException("A social challenge can be activated only within its daily time window.");
        }

        if (ActiveRun is not null && ActiveRun.HasWindowEnded(TimeWindow, moment))
        {
            ActiveRun = null;
        }
        ActiveRun ??= new SocialChallengeRun(moment);
        ActiveRun.Join(touristId, moment);
    }

    // Records the tourist's check and returns the tourists who solve the challenge with it, or
    // an empty list. A solved run and a run whose window has ended are closed.
    public List<Guid> CheckPosition(Guid touristId, Location position, DateTime moment)
    {
        EnsurePublished();
        if (ActiveRun is null)
        {
            return [];
        }
        if (ActiveRun.HasWindowEnded(TimeWindow, moment))
        {
            ActiveRun = null;
            return [];
        }

        var isInRange = IsWithinRadius(Location!, RangeInMeters, position);
        var solvers = ActiveRun.Check(touristId, isInRange, moment, RequiredPeople);
        if (solvers.Count > 0)
        {
            ActiveRun = null;
        }
        return solvers;
    }

    // Derives what the tourist sees without changing anything; a closed window shows the run as
    // closed.
    public RunState RunStateFor(Guid touristId, DateTime moment)
    {
        if (!TimeWindow.IsOpenAt(moment))
        {
            return new RunState(RunStatus.Closed, 0);
        }
        if (ActiveRun is null || ActiveRun.HasWindowEnded(TimeWindow, moment))
        {
            return new RunState(RunStatus.Left, 0);
        }
        return ActiveRun.StateFor(touristId, moment);
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

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

    // A participant who has not checked in for this long leaves the run.
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(30);

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

    public bool IsOpenAt(TimeOnly moment)
    {
        return TimeWindow.Contains(moment);
    }

    // The moment is in the city's local time. A tourist joins the active run, or starts a new
    // one when there is none or the window of the last one has ended.
    public void Activate(Guid touristId, Location position, DateTime moment)
    {
        EnsureCanBeActivatedFrom(position);
        if (!IsOpenAt(TimeOnly.FromDateTime(moment)))
        {
            throw new DomainException("A social challenge can be activated only within its daily time window.");
        }

        if (ActiveRun is not null && HasRunWindowEnded(moment))
        {
            ActiveRun = null;
        }
        ActiveRun ??= new SocialChallengeRun(moment);
        ActiveRun.Join(touristId, moment);
    }

    // Records the tourist's check and returns the tourists who solve the challenge with it, or
    // an empty list. A tourist who is not in the run changes nothing for themselves, because
    // another participant's check may already have solved or closed it.
    public List<Guid> CheckPosition(Guid touristId, Location position, DateTime moment)
    {
        if (ActiveRun is null)
        {
            return [];
        }
        if (HasRunWindowEnded(moment))
        {
            ActiveRun = null;
            return [];
        }

        var participant = ActiveRun.Find(touristId);
        if (participant is not null)
        {
            if (Location is null || Location.DistanceTo(position) > RangeInMeters)
            {
                ActiveRun.Remove(participant);
            }
            else
            {
                participant.RecordCheck(moment);
            }
        }
        ActiveRun.RemoveInactive(moment, CheckTimeout);

        if (ActiveRun.Participants.Count < RequiredPeople)
        {
            return [];
        }
        var solvers = ActiveRun.Participants.Select(solver => solver.TouristId).ToList();
        ActiveRun = null;
        return solvers;
    }

    // Derives what the tourist sees without changing anything: participants who have not checked
    // in for the timeout are not counted, and a closed window shows the run as closed.
    public RunState RunStateFor(Guid touristId, DateTime moment)
    {
        if (!IsOpenAt(TimeOnly.FromDateTime(moment)))
        {
            return new RunState(RunStatus.Closed, 0);
        }
        if (ActiveRun is null || HasRunWindowEnded(moment))
        {
            return new RunState(RunStatus.Left, 0);
        }

        var participant = ActiveRun.Find(touristId);
        var status = participant is not null && participant.IsActiveAt(moment, CheckTimeout)
            ? RunStatus.Active
            : RunStatus.Left;
        return new RunState(status, ActiveRun.CountActive(moment, CheckTimeout));
    }

    // A run starts inside the window, so its window ends at the first end time after the start;
    // subtracting two TimeOnly values wraps around midnight.
    private bool HasRunWindowEnded(DateTime moment)
    {
        var windowEndsAt = ActiveRun!.StartedAt + (TimeWindow.End - TimeOnly.FromDateTime(ActiveRun.StartedAt));
        return moment >= windowEndsAt;
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

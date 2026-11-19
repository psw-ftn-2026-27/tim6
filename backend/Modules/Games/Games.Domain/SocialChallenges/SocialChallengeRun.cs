using Shared.Domain;

namespace Games.Domain.SocialChallenges;

// The one active run of a social challenge: when it started, who takes part in it, and how the
// participants' checks decide who is still in. Every moment is in UTC.
public sealed class SocialChallengeRun : Entity
{
    // A participant who has not checked in for this long leaves the run.
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(30);

    private readonly List<RunParticipant> _participants = [];

    public DateTime StartedAt { get; private set; }
    public IReadOnlyList<RunParticipant> Participants => _participants;

    private SocialChallengeRun() { }

    public SocialChallengeRun(DateTime startedAt)
        : base(Guid.NewGuid())
    {
        StartedAt = startedAt;
    }

    // A tourist who is already a participant only records a new check.
    public void Join(Guid touristId, DateTime moment)
    {
        var participant = Find(touristId);
        if (participant is null)
        {
            _participants.Add(new RunParticipant(touristId, moment));
            return;
        }
        participant.RecordCheck(moment);
    }

    public bool HasWindowEnded(DailyTimeWindow timeWindow, DateTime moment)
    {
        return timeWindow.HasEndedSince(StartedAt, moment);
    }

    // Records the tourist's check, or removes them when they are out of range, then removes the
    // participants silent for the timeout. Returns the remaining participants when there are
    // enough of them to solve the challenge, or an empty list. A tourist who is not in the run
    // changes nothing for themselves, because another participant's check may already have
    // solved or closed it.
    public List<Guid> Check(Guid touristId, bool isInRange, DateTime moment, int requiredPeople)
    {
        var participant = Find(touristId);
        if (participant is not null && isInRange)
        {
            participant.RecordCheck(moment);
        }
        else if (participant is not null)
        {
            _participants.Remove(participant);
        }
        _participants.RemoveAll(other => !other.IsActiveAt(moment, CheckTimeout));

        if (_participants.Count < requiredPeople)
        {
            return [];
        }
        return _participants.Select(solver => solver.TouristId).ToList();
    }

    // What the tourist sees without changing anything: participants silent for the timeout are
    // not counted.
    public RunState StateFor(Guid touristId, DateTime moment)
    {
        var participant = Find(touristId);
        var status = participant is not null && participant.IsActiveAt(moment, CheckTimeout)
            ? RunStatus.Active
            : RunStatus.Left;
        return new RunState(status, _participants.Count(other => other.IsActiveAt(moment, CheckTimeout)));
    }

    private RunParticipant? Find(Guid touristId)
    {
        return _participants.FirstOrDefault(participant => participant.TouristId == touristId);
    }
}

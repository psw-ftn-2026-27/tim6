using Shared.Domain;

namespace Games.Domain.SocialChallenges;

// The one active run of a social challenge: when it started and who takes part in it.
public sealed class SocialChallengeRun : Entity
{
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

    public RunParticipant? Find(Guid touristId)
    {
        return _participants.FirstOrDefault(participant => participant.TouristId == touristId);
    }

    public void Remove(RunParticipant participant)
    {
        _participants.Remove(participant);
    }

    public void RemoveInactive(DateTime moment, TimeSpan timeout)
    {
        _participants.RemoveAll(participant => !participant.IsActiveAt(moment, timeout));
    }

    public int CountActive(DateTime moment, TimeSpan timeout)
    {
        return _participants.Count(participant => participant.IsActiveAt(moment, timeout));
    }
}

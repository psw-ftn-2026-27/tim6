using Shared.Domain;

namespace Games.Domain.SocialChallenges;

// A tourist in the active run of a social challenge. The last position is not kept, because
// a participant found out of range leaves the run at once.
public sealed class RunParticipant : Entity
{
    public Guid TouristId { get; private set; }
    public DateTime LastCheckedAt { get; private set; }

    private RunParticipant() { }

    public RunParticipant(Guid touristId, DateTime checkedAt)
        : base(Guid.NewGuid())
    {
        TouristId = touristId;
        LastCheckedAt = checkedAt;
    }

    public void RecordCheck(DateTime checkedAt)
    {
        LastCheckedAt = checkedAt;
    }

    // A participant who has not checked in for the timeout is no longer active.
    public bool IsActiveAt(DateTime moment, TimeSpan timeout)
    {
        return moment - LastCheckedAt < timeout;
    }
}

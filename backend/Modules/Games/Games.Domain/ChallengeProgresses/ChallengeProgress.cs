using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.ChallengeProgresses;

// What one tourist has achieved on one challenge of any kind: the best XP earned so far and
// whether the challenge is solved. It knows the challenge and the tourist only by their IDs.
public sealed class ChallengeProgress : AggregateRoot
{
    public Guid ChallengeId { get; private set; }
    public Guid TouristId { get; private set; }
    public int EarnedXp { get; private set; }
    public DateTime? SolvedAt { get; private set; }

    private ChallengeProgress() { }

    public ChallengeProgress(Guid challengeId, Guid touristId)
        : base(Guid.NewGuid())
    {
        ChallengeId = challengeId;
        TouristId = touristId;
    }

    public bool IsSolved()
    {
        return SolvedAt is not null;
    }

    public void EnsureNotSolved()
    {
        if (IsSolved())
        {
            throw new DomainException("A solved challenge cannot be activated again.");
        }
    }

    // Awards only the XP above what the tourist already earned on the challenge and returns it.
    public int RecordResult(int xp, bool solves, DateTime recordedAt)
    {
        if (IsSolved())
        {
            throw new DomainException("A solved challenge cannot be attempted again.");
        }

        var awardedXp = Math.Max(0, xp - EarnedXp);
        EarnedXp += awardedXp;
        if (solves)
        {
            SolvedAt = recordedAt;
        }

        return awardedXp;
    }
}

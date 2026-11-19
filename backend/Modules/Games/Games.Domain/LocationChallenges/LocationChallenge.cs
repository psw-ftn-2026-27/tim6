using Games.Domain.Challenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallenge : Challenge
{
    private const int MaxNameLength = 200;
    private const int MinHiddenPoints = 1;

    private readonly List<HiddenPoint> _hiddenPoints = [];

    public string Name { get; private set; }
    public int Xp { get; private set; }
    public IReadOnlyList<HiddenPoint> HiddenPoints => _hiddenPoints;

    protected override string KindName => "location challenge";

    // The same fixed activation radius as a quiz (G22 requirements, to be confirmed for G21).
    public override int ActivationRadiusInMeters => 50;

    private LocationChallenge()
    {
        Name = null!;
    }

    public LocationChallenge(string name, int xp, List<HiddenPoint> hiddenPoints)
        : base(Guid.NewGuid())
    {
        Validate(name, xp, hiddenPoints);

        Name = name;
        Xp = xp;
        _hiddenPoints.AddRange(hiddenPoints);
    }

    public void Edit(string name, int xp, List<HiddenPoint> hiddenPoints)
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft location challenge can be edited.");
        }
        Validate(name, xp, hiddenPoints);

        Name = name;
        Xp = xp;
        _hiddenPoints.Clear();
        _hiddenPoints.AddRange(hiddenPoints);
    }

    private static void Validate(string name, int xp, List<HiddenPoint> hiddenPoints)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A location challenge requires a name.");
        }
        if (name.Length > MaxNameLength)
        {
            throw new DomainException($"A location challenge name has at most {MaxNameLength} characters.");
        }
        if (xp <= 0)
        {
            throw new DomainException("A location challenge rewards a positive amount of XP.");
        }
        if (hiddenPoints is null || hiddenPoints.Count < MinHiddenPoints)
        {
            throw new DomainException($"A location challenge has at least {MinHiddenPoints} hidden point.");
        }
    }
}

using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourPreferences;

public sealed class TourPreference : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public List<TourDifficulty> Difficulties { get; private set; }
    public TransportMode Transport { get; private set; }
    public List<string> Tags { get; private set; }

    private TourPreference()
    {
        Difficulties = null!;
        Tags = null!;
    }

    public TourPreference(Guid touristId, List<TourDifficulty> difficulties, TransportMode transport, List<string> tags)
        : base(Guid.NewGuid())
    {
        Validate(difficulties, tags);

        TouristId = touristId;
        Difficulties = difficulties;
        Transport = transport;
        Tags = tags;
    }

    public void Update(List<TourDifficulty> difficulties, TransportMode transport, List<string> tags)
    {
        Validate(difficulties, tags);

        Difficulties = difficulties;
        Transport = transport;
        Tags = tags;
    }

    private static void Validate(List<TourDifficulty> difficulties, List<string> tags)
    {
        if (difficulties is null || difficulties.Count == 0)
        {
            throw new DomainException("A tour preference requires at least one difficulty.");
        }
        if (difficulties.Distinct().Count() != difficulties.Count)
        {
            throw new DomainException("A tour preference cannot repeat a difficulty.");
        }
        if (tags is null)
        {
            throw new DomainException("A tour preference requires a list of tags, which may be empty.");
        }
        if (tags.Distinct().Count() != tags.Count)
        {
            throw new DomainException("A tour preference cannot repeat a tag.");
        }
    }
}

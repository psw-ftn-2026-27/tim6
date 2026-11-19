using Games.Domain.Challenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class HiddenPoint : Entity
{
    public string Image { get; private set; }
    public Location Location { get; private set; }

    private HiddenPoint()
    {
        Image = null!;
        Location = null!;
    }

    public HiddenPoint(string image, Location location)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            throw new DomainException("A hidden point requires an image.");
        }
        if (location is null)
        {
            throw new DomainException("A hidden point requires a location.");
        }

        Image = image;
        Location = location;
    }
}

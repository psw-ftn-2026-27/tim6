using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class KeyPoint : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Image { get; private set; }
    public string? Secret { get; private set; }
    public Location Location { get; private set; }
    public bool IsVisibleWithoutPurchase { get; private set; }
    public int Order { get; private set; }

    private KeyPoint()
    {
        Name = null!;
        Description = null!;
        Image = null!;
        Location = null!;
    }

    internal KeyPoint(string name, string description, string image, string? secret, Location location,
        bool isVisibleWithoutPurchase, int order)
        : base(Guid.NewGuid())
    {
        EnsureValidContent(name, description, image, location);

        Name = name;
        Description = description;
        Image = image;
        Secret = secret;
        Location = location;
        IsVisibleWithoutPurchase = isVisibleWithoutPurchase;
        Order = order;
    }

    internal void Update(string name, string description, string image, string? secret, Location location,
        bool isVisibleWithoutPurchase)
    {
        EnsureValidContent(name, description, image, location);

        Name = name;
        Description = description;
        Image = image;
        Secret = secret;
        Location = location;
        IsVisibleWithoutPurchase = isVisibleWithoutPurchase;
    }

    internal void MoveTo(int order)
    {
        Order = order;
    }

    private static void EnsureValidContent(string name, string description, string image, Location location)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A key point requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A key point requires a description.");
        }
        if (string.IsNullOrWhiteSpace(image))
        {
            throw new DomainException("A key point requires an image.");
        }
        if (location is null)
        {
            throw new DomainException("A key point requires a location.");
        }
    }
}

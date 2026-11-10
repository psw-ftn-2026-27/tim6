using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    private const int MaxImages = 5;

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<string> Images { get; private set; }

    private Club()
    {
        Name = null!;
        Description = null!;
        Images = null!;
    }

    public Club(Guid ownerId, string name, string description, List<string> images)
        : base(Guid.NewGuid())
    {
        Validate(name, description, images);

        OwnerId = ownerId;
        Name = name;
        Description = description;
        Images = images;
    }

    public void Update(Guid userId, string name, string description, List<string> images)
    {
        EnsureOwner(userId);
        Validate(name, description, images);

        Name = name;
        Description = description;
        Images = images;
    }

    public void Delete(Guid userId)
    {
        EnsureOwner(userId);
    }

    private void EnsureOwner(Guid userId)
    {
        if (userId != OwnerId)
        {
            throw new NotFoundException($"Club {Id} does not exist.");
        }
    }

    private static void Validate(string name, string description, List<string> images)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A club requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A club requires a description.");
        }
        if (images.Count == 0)
        {
            throw new DomainException("A club requires at least one image.");
        }
        if (images.Count > MaxImages)
        {
            throw new DomainException($"A club can have at most {MaxImages} images.");
        }
    }
}

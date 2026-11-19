using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class ClubMember : Entity
{
    public Guid UserId { get; private set; }
    public ClubRole Role { get; private set; }

    private ClubMember() { }

    public ClubMember(Guid userId, ClubRole role) : base(Guid.NewGuid())
    {
        UserId = userId;
        Role = role;
    }

    public void PromoteToModerator()
    {
        if (Role != ClubRole.Member)
        {
            throw new DomainException("Only a plain member can be promoted to moderator.");
        }

        Role = ClubRole.Moderator;
    }

    public void TakeOverOwnership()
    {
        if (Role == ClubRole.Owner)
        {
            throw new DomainException("The owner cannot take over ownership again.");
        }

        Role = ClubRole.Owner;
    }

    public void HandOverOwnership()
    {
        Role = ClubRole.Moderator;
    }

    public bool CanResolveJoinRequests()
    {
        return Role == ClubRole.Owner || Role == ClubRole.Moderator;
    }

    public bool CanRemove(ClubMember member)
    {
        if (Role == ClubRole.Owner)
        {
            return member.Role != ClubRole.Owner;
        }
        if (Role == ClubRole.Moderator)
        {
            return member.Role == ClubRole.Member;
        }
        return false;
    }
}

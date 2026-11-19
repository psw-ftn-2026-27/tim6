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
    public DateTime CreatedAt { get; private set; }
    public ClubStatus Status { get; private set; }
    private readonly List<ClubMember> _members = [];
    public IReadOnlyList<ClubMember> Members => _members;
    private readonly List<ClubJoinRequest> _joinRequests = [];
    public IReadOnlyList<ClubJoinRequest> JoinRequests => _joinRequests;

    private Club()
    {
        Name = null!;
        Description = null!;
        Images = null!;
    }

    public Club(Guid ownerId, string name, string description, List<string> images, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        Validate(name, description, images);

        OwnerId = ownerId;
        Name = name;
        Description = description;
        Images = images;
        CreatedAt = createdAt;
        Status = ClubStatus.Active;
        _members.Add(new ClubMember(ownerId, ClubRole.Owner));
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

    public void RequestToJoin(Guid userId)
    {
        EnsureActive();
        if (FindMember(userId) is not null)
        {
            throw new DomainException("A member cannot request to join the club.");
        }
        if (_joinRequests.Any(request => request.UserId == userId))
        {
            throw new DomainException("A join request to this club is already pending.");
        }

        _joinRequests.Add(new ClubJoinRequest(userId, DateTime.UtcNow));
    }

    public void WithdrawJoinRequest(Guid userId)
    {
        RemovePendingJoinRequest(userId);
    }

    public void AcceptJoinRequest(Guid resolverId, Guid applicantId)
    {
        EnsureCanResolve(resolverId);
        EnsureActive();
        RemovePendingJoinRequest(applicantId);
        _members.Add(new ClubMember(applicantId, ClubRole.Member));
    }

    public void RejectJoinRequest(Guid resolverId, Guid applicantId)
    {
        EnsureCanResolve(resolverId);
        EnsureActive();
        RemovePendingJoinRequest(applicantId);
    }

    public void PromoteToModerator(Guid promoterId, Guid memberId)
    {
        EnsureActive();
        if (promoterId != OwnerId)
        {
            throw new DomainException("Only the owner of the club can promote a member to moderator.");
        }

        GetMember(memberId).PromoteToModerator();
    }

    public void RemoveMember(Guid removerId, Guid memberId)
    {
        EnsureActive();
        var member = GetMember(memberId);
        var remover = FindMember(removerId);
        if (remover is null || !remover.CanRemove(member))
        {
            throw new DomainException("The owner can remove any other member, and a moderator only a plain member.");
        }

        _members.Remove(member);
    }

    public void TransferOwnership(Guid userId, Guid newOwnerId)
    {
        EnsureActive();
        if (userId != OwnerId)
        {
            throw new DomainException("Only the owner of the club can transfer ownership.");
        }

        GetMember(newOwnerId).TakeOverOwnership();
        GetMember(userId).HandOverOwnership();
        OwnerId = newOwnerId;
    }

    public void Close(Guid userId)
    {
        if (userId != OwnerId)
        {
            throw new DomainException("Only the owner of the club can close it.");
        }
        EnsureActive();

        Status = ClubStatus.Closed;
        _joinRequests.Clear();
    }

    public bool CanResolveJoinRequests(Guid userId)
    {
        var member = FindMember(userId);
        return member is not null && member.CanResolveJoinRequests();
    }

    private void RemovePendingJoinRequest(Guid applicantId)
    {
        var request = _joinRequests.FirstOrDefault(request => request.UserId == applicantId);
        if (request is null)
        {
            throw new DomainException("There is no pending join request from this user.");
        }

        _joinRequests.Remove(request);
    }

    private void EnsureActive()
    {
        if (Status != ClubStatus.Active)
        {
            throw new DomainException("This action is possible only while the club is active.");
        }
    }

    private ClubMember? FindMember(Guid userId)
    {
        return _members.FirstOrDefault(member => member.UserId == userId);
    }

    private ClubMember GetMember(Guid memberId)
    {
        var member = FindMember(memberId);
        if (member is null)
        {
            throw new DomainException("This user is not a member of the club.");
        }
        return member;
    }

    private void EnsureCanResolve(Guid resolverId)
    {
        if (!CanResolveJoinRequests(resolverId))
        {
            throw new DomainException("Only the owner or a moderator of the club can resolve join requests.");
        }
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

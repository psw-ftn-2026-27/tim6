using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubAuthoring;

public sealed class ClubAuthoringService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubAuthoringService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(Guid ownerId, CreateClubDto dto)
    {
        var club = new Club(ownerId, dto.Name, dto.Description, dto.Images, DateTime.UtcNow);

        _clubRepository.Add(club);
        await _unitOfWork.SaveChangesAsync();

        return club.Id;
    }

    public async Task UpdateAsync(Guid clubId, Guid userId, UpdateClubDto dto)
    {
        var club = await GetClubAsync(clubId);

        club.Update(userId, dto.Name, dto.Description, dto.Images);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.Delete(userId);
        _clubRepository.Remove(club);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetClubAsync(Guid clubId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }
}

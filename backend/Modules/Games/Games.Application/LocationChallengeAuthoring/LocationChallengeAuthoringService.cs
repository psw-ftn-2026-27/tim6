using Games.Application.LocationChallenges;
using Games.Domain.Challenges;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeAuthoring;

public sealed class LocationChallengeAuthoringService
{
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengeAuthoringService(ILocationChallengeRepository challengeRepository, IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(CreateLocationChallengeDto dto)
    {
        var challenge = new LocationChallenge(dto.Name, dto.Xp, ToHiddenPoints(dto.HiddenPoints));
        if (dto.Location is not null)
        {
            challenge.PlaceAt(new Location(dto.Location.Latitude, dto.Location.Longitude));
        }

        _challengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return challenge.Id;
    }

    public async Task EditAsync(Guid challengeId, UpdateLocationChallengeDto dto)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Edit(dto.Name, dto.Xp, ToHiddenPoints(dto.HiddenPoints));
        if (dto.Location is null)
        {
            challenge.RemoveLocation();
        }
        else
        {
            challenge.PlaceAt(new Location(dto.Location.Latitude, dto.Location.Longitude));
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Delete();
        _challengeRepository.Remove(challenge);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<LocationChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Location challenge {challengeId} does not exist.");
        }
        return challenge;
    }

    private static List<HiddenPoint> ToHiddenPoints(List<HiddenPointDto> hiddenPoints)
    {
        return hiddenPoints
            .Select(point => new HiddenPoint(point.Image, new Location(point.Location.Latitude, point.Location.Longitude)))
            .ToList();
    }
}

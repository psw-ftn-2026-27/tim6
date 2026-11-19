using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengePublishing;

public sealed class LocationChallengePublishingService
{
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengePublishingService(ILocationChallengeRepository challengeRepository, IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PublishAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RetireAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Retire();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Archive();
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
}

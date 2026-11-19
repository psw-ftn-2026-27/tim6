using Games.Application.ChallengeProgresses;
using Games.Application.Challenges;
using Games.Application.SocialChallenges;
using Games.Domain.ChallengeProgresses;
using Games.Domain.Challenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeSolving;

public sealed class SocialChallengeSolvingService
{
    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly IChallengeProgressRepository _progressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengeSolvingService(
        ISocialChallengeRepository challengeRepository,
        IChallengeProgressRepository progressRepository,
        IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _progressRepository = progressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid challengeId, Guid touristId, LocationDto position)
    {
        var challenge = await GetChallengeAsync(challengeId);
        var progress = await _progressRepository.GetAsync(challenge.Id, touristId);
        progress?.EnsureNotSolved();

        challenge.Activate(touristId, new Location(position.Latitude, position.Longitude), DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CheckPositionAsync(Guid challengeId, Guid touristId, LocationDto position)
    {
        var challenge = await GetChallengeAsync(challengeId);

        var now = DateTime.UtcNow;
        var solvers = challenge.CheckPosition(touristId, new Location(position.Latitude, position.Longitude), now);

        foreach (var solverId in solvers)
        {
            var progress = await _progressRepository.GetAsync(challenge.Id, solverId);
            if (progress is null)
            {
                progress = new ChallengeProgress(challenge.Id, solverId);
                _progressRepository.Add(progress);
            }
            progress.RecordResult(challenge.Xp, true, now);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<SocialChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {challengeId} does not exist.");
        }
        return challenge;
    }
}

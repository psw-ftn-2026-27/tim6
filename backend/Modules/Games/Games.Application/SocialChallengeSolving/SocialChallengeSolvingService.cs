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
    // Daily time windows are set in the city's local time.
    private static readonly TimeZoneInfo CityTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");

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

        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CityTimeZone);
        challenge.Activate(touristId, new Location(position.Latitude, position.Longitude), now);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CheckPositionAsync(Guid challengeId, Guid touristId, LocationDto position)
    {
        var challenge = await GetChallengeAsync(challengeId);

        var utcNow = DateTime.UtcNow;
        var now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, CityTimeZone);
        var solvers = challenge.CheckPosition(touristId, new Location(position.Latitude, position.Longitude), now);

        foreach (var solverId in solvers)
        {
            var progress = await _progressRepository.GetAsync(challenge.Id, solverId);
            if (progress is null)
            {
                progress = new ChallengeProgress(challenge.Id, solverId);
                _progressRepository.Add(progress);
            }
            progress.RecordResult(challenge.Xp, true, utcNow);
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

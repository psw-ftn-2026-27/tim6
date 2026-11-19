using Games.Application.Challenges;
using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallenges;
using Games.Domain.Challenges;
using Games.Domain.LocationChallengeAttempts;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeSolving;

public sealed class LocationChallengeSolvingService
{
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly ILocationChallengeAttemptRepository _attemptRepository;
    private readonly LocationChallengeSolver _solver;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengeSolvingService(
        ILocationChallengeRepository challengeRepository,
        ILocationChallengeAttemptRepository attemptRepository,
        LocationChallengeSolver solver,
        IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _attemptRepository = attemptRepository;
        _solver = solver;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ActivateAsync(Guid explorerId, Guid challengeId, LocationDto position)
    {
        var challenge = await GetChallengeAsync(challengeId);
        var existingAttempt = await _attemptRepository.GetByChallengeAndExplorerAsync(challengeId, explorerId);
        var extraMeters = Random.Shared.NextDouble() * LocationChallengeSolver.MaxExtraMeters;

        var attempt = _solver.Activate(challenge, existingAttempt, explorerId,
            new Location(position.Latitude, position.Longitude), extraMeters);
        if (attempt != existingAttempt)
        {
            _attemptRepository.Add(attempt);
        }
        await _unitOfWork.SaveChangesAsync();

        return attempt.Id;
    }

    public async Task CheckLocationAsync(Guid explorerId, Guid challengeId, LocationDto position)
    {
        var challenge = await GetChallengeAsync(challengeId);
        var attempt = await _attemptRepository.GetByChallengeAndExplorerAsync(challengeId, explorerId);
        if (attempt is null)
        {
            throw new NotFoundException($"You have not activated location challenge {challengeId}.");
        }

        _solver.CheckLocation(challenge, attempt, new Location(position.Latitude, position.Longitude));
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

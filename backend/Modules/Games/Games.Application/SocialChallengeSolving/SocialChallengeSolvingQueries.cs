using Games.Application.ChallengeProgresses;
using Games.Application.Challenges;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeSolving;

public sealed class SocialChallengeSolvingQueries
{
    // Daily time windows are set in the city's local time.
    private static readonly TimeZoneInfo CityTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");

    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly IChallengeProgressRepository _progressRepository;

    public SocialChallengeSolvingQueries(
        ISocialChallengeRepository challengeRepository,
        IChallengeProgressRepository progressRepository)
    {
        _challengeRepository = challengeRepository;
        _progressRepository = progressRepository;
    }

    // A tourist who solved the challenge sees the earned XP and the exit message; everyone else
    // sees their place in the active run.
    public async Task<SocialChallengeRunStateDto> GetRunStateAsync(Guid challengeId, Guid touristId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {challengeId} does not exist.");
        }
        var progress = await _progressRepository.GetAsync(challenge.Id, touristId);

        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CityTimeZone);
        var runState = challenge.RunStateFor(touristId, now);
        var solved = progress is not null && progress.IsSolved();

        return new SocialChallengeRunStateDto(
            challenge.Id,
            challenge.Name,
            challenge.Location is null ? null : new LocationDto(challenge.Location.Latitude, challenge.Location.Longitude),
            challenge.RangeInMeters,
            solved ? SolvingStatus.Solved : ToSolvingStatus(runState.Status),
            runState.ActiveCount,
            challenge.RequiredPeople,
            solved ? progress!.EarnedXp : null,
            solved ? challenge.ExitMessage : null);
    }

    private static SolvingStatus ToSolvingStatus(RunStatus status)
    {
        return status switch
        {
            RunStatus.Active => SolvingStatus.Active,
            RunStatus.Left => SolvingStatus.Left,
            _ => SolvingStatus.Closed
        };
    }
}

using Games.Application.Challenges;
using Games.Application.Quizzes;
using Games.Application.SocialChallenges;
using Games.Domain.Challenges;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;

namespace Games.Application.ChallengeMap;

public sealed class ChallengeMapQueries
{
    private readonly IQuizReadRepository _quizReadRepository;
    private readonly ISocialChallengeReadRepository _challengeReadRepository;

    public ChallengeMapQueries(
        IQuizReadRepository quizReadRepository,
        ISocialChallengeReadRepository challengeReadRepository)
    {
        _quizReadRepository = quizReadRepository;
        _challengeReadRepository = challengeReadRepository;
    }

    // The position is known only when both coordinates are given.
    public async Task<List<ChallengeOnMapDto>> GetChallengeMapAsync(Guid touristId, double? latitude, double? longitude)
    {
        var position = latitude is null || longitude is null ? null : new Location(latitude.Value, longitude.Value);
        var now = DateTime.UtcNow;

        var quizzes = await _quizReadRepository.GetPublishedForMapAsync(touristId);
        var socialChallenges = await _challengeReadRepository.GetPublishedForMapAsync(touristId);

        var quizzesOnMap = quizzes.Select(quiz =>
        {
            var location = ToLocation(quiz.Location);
            return new ChallengeOnMapDto(
                quiz.Id,
                ChallengeKind.Quiz,
                quiz.Name,
                quiz.Xp,
                quiz.Location,
                Quiz.ActivationRadius,
                quiz.IsSolved,
                position?.DistanceTo(location),
                Quiz.CanBeActivated(location, position, quiz.IsSolved));
        });
        var socialChallengesOnMap = socialChallenges.Select(challenge =>
        {
            var location = ToLocation(challenge.Location);
            var timeWindow = new DailyTimeWindow(challenge.WindowStart, challenge.WindowEnd);
            return new ChallengeOnMapDto(
                challenge.Id,
                ChallengeKind.SocialChallenge,
                challenge.Name,
                challenge.Xp,
                challenge.Location,
                challenge.RangeInMeters,
                challenge.IsSolved,
                position?.DistanceTo(location),
                SocialChallenge.CanBeActivated(location, challenge.RangeInMeters, timeWindow, position, challenge.IsSolved, now));
        });

        return quizzesOnMap.Concat(socialChallengesOnMap).ToList();
    }

    private static Location ToLocation(LocationDto location)
    {
        return new Location(location.Latitude, location.Longitude);
    }
}

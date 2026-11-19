using Games.Application.Challenges;
using Games.Application.Quizzes;
using Games.Application.SocialChallenges;
using Games.Domain.Challenges;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;

namespace Games.Application.ChallengeMap;

public sealed class ChallengeMapQueries
{
    // Daily time windows are set in the city's local time.
    private static readonly TimeZoneInfo CityTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");

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
        var now = TimeOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CityTimeZone));

        var quizzes = await _quizReadRepository.GetPublishedForMapAsync(touristId);
        var socialChallenges = await _challengeReadRepository.GetPublishedForMapAsync(touristId);

        var quizzesOnMap = quizzes.Select(quiz =>
        {
            var distance = DistanceFrom(position, quiz.Location);
            return new ChallengeOnMapDto(
                quiz.Id,
                ChallengeKind.Quiz,
                quiz.Name,
                quiz.Xp,
                quiz.Location,
                Quiz.ActivationRadius,
                quiz.IsSolved,
                distance,
                !quiz.IsSolved && distance is not null && distance <= Quiz.ActivationRadius);
        });
        var socialChallengesOnMap = socialChallenges.Select(challenge =>
        {
            var distance = DistanceFrom(position, challenge.Location);
            var timeWindow = new DailyTimeWindow(challenge.WindowStart, challenge.WindowEnd);
            return new ChallengeOnMapDto(
                challenge.Id,
                ChallengeKind.SocialChallenge,
                challenge.Name,
                challenge.Xp,
                challenge.Location,
                challenge.RangeInMeters,
                challenge.IsSolved,
                distance,
                !challenge.IsSolved && distance is not null && distance <= challenge.RangeInMeters && timeWindow.Contains(now));
        });

        return quizzesOnMap.Concat(socialChallengesOnMap).ToList();
    }

    private static double? DistanceFrom(Location? position, LocationDto challengeLocation)
    {
        return position?.DistanceTo(new Location(challengeLocation.Latitude, challengeLocation.Longitude));
    }
}

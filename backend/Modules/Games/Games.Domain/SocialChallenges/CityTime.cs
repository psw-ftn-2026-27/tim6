namespace Games.Domain.SocialChallenges;

// Daily time windows are set in the city's local time. Every moment is kept in UTC and turned
// into the city's time only here, when a window is checked, so a daylight saving change never
// shifts a stored moment.
public static class CityTime
{
    private static readonly TimeZoneInfo CityTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");

    public static DateTime FromUtc(DateTime utcMoment)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcMoment, CityTimeZone);
    }
}

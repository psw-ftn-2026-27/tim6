namespace Social.Tests.Integration.Seeds;

internal static class SocialSeed
{
    public static object[] All => [.. BlogSeed.All, .. FollowSeed.All, .. FollowRequestSeed.All, .. ClubSeed.All];
}

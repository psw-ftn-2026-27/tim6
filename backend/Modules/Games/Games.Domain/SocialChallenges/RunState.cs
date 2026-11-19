namespace Games.Domain.SocialChallenges;

// What a tourist sees of the active run: their own status and how many participants are active.
public sealed record RunState(RunStatus Status, int ActiveCount);

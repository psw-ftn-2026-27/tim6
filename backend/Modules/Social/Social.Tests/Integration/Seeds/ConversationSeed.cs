using Shared.Domain.Conversations;
using Shared.Tests;

namespace Social.Tests.Integration.Seeds;

internal static class ConversationSeed
{
    public static readonly Conversation Direct = Conversation.CreateDirect(
        WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
    public static readonly ChatMessage ExplorersMessage = Direct.CreateMessage(
        WellKnownUsers.Explorer, "Pozdrav iz direktnog razgovora.", DateTime.UtcNow.AddMinutes(-2));
    public static readonly ChatMessage SecondExplorersMessage = Direct.CreateMessage(
        WellKnownUsers.SecondExplorer, "Odgovor iz direktnog razgovora.", DateTime.UtcNow.AddMinutes(-1));

    public static object[] All => [Direct, ExplorersMessage, SecondExplorersMessage];
}

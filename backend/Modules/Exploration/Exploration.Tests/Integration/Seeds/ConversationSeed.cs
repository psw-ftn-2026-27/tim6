using Shared.Domain.Conversations;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class ConversationSeed
{
    public static readonly Conversation TourConversation;
    public static readonly ChatMessage BuyersMessage;
    public static readonly ChatMessage AuthorsMessage;

    static ConversationSeed()
    {
        TourConversation = Conversation.CreateContextual(
            ConversationContext.Contextual("tour", TourSeed.FortressWalk.Id), [WellKnownUsers.Explorer]);
        TourConversation.AddParticipant(WellKnownUsers.SecondExplorer);
        BuyersMessage = TourConversation.CreateMessage(WellKnownUsers.SecondExplorer,
            "Pitanje o turi.", DateTime.UtcNow.AddMinutes(-2));
        AuthorsMessage = TourConversation.CreateMessage(WellKnownUsers.Explorer,
            "Odgovor autora.", DateTime.UtcNow.AddMinutes(-1));
    }

    public static object[] All => [TourConversation, BuyersMessage, AuthorsMessage];
}

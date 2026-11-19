using Shared.Domain.Conversations;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class ConversationSeed
{
    public static readonly Conversation TourConversation;
    public static readonly Conversation MutedTourConversation;
    public static readonly Conversation PermanentlyMutedTourConversation;
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

        MutedTourConversation = Conversation.CreateContextual(
            ConversationContext.Contextual("tour", TourSeed.CityStroll.Id), [WellKnownUsers.Explorer]);
        MutedTourConversation.AddParticipant(WellKnownUsers.SecondExplorer);
        MutedTourConversation.Mute(
            WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, MuteDuration.OneHour, DateTime.UtcNow);

        PermanentlyMutedTourConversation = Conversation.CreateContextual(
            ConversationContext.Contextual("tour", TourSeed.PublishableRiverside.Id), [WellKnownUsers.Explorer]);
        PermanentlyMutedTourConversation.AddParticipant(WellKnownUsers.SecondExplorer);
        PermanentlyMutedTourConversation.Mute(
            WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, MuteDuration.Permanent, DateTime.UtcNow);
    }

    public static object[] All =>
    [
        TourConversation,
        MutedTourConversation,
        PermanentlyMutedTourConversation,
        BuyersMessage,
        AuthorsMessage
    ];
}

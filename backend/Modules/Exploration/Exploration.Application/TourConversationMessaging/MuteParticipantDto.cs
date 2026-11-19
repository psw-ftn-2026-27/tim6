using Shared.Domain.Conversations;

namespace Exploration.Application.TourConversationMessaging;

public sealed record MuteParticipantDto(Guid ParticipantId, MuteDuration Duration);

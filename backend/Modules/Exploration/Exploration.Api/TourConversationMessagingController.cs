using Exploration.Application.Conversations;
using Exploration.Application.TourConversationMessaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/conversations")]
[Authorize]
public sealed class TourConversationMessagingController : ControllerBase
{
    private readonly TourConversationMessagingService _service;
    private readonly TourConversationMessagingQueries _queries;

    public TourConversationMessagingController(
        TourConversationMessagingService service, TourConversationMessagingQueries queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpPost("tour/{tourId:guid}")]
    public async Task<ActionResult<Guid>> OpenTour(Guid tourId) =>
        await _service.OpenTourAsync(User.GetUserId(), tourId);

    [HttpGet("{conversationId:guid}")]
    public async Task<ActionResult<ConversationDto>> Get(Guid conversationId) =>
        await _queries.GetConversationAsync(User.GetUserId(), conversationId);

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<MessagePageDto>> GetMessages(
        Guid conversationId, [FromQuery] DateTime? cursorCreatedAt = null,
        [FromQuery] Guid? cursorId = null, [FromQuery] bool afterCursor = false,
        [FromQuery] int pageSize = 20)
    {
        var cursor = cursorCreatedAt is null || cursorId is null
            ? null : new MessageCursorDto(cursorCreatedAt.Value, cursorId.Value);
        return await _queries.GetMessagesAsync(User.GetUserId(), conversationId, cursor, afterCursor, pageSize);
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<ActionResult<Guid>> Send(Guid conversationId, SendMessageDto dto) =>
        await _service.SendAsync(User.GetUserId(), conversationId, dto.Content);

    [HttpDelete("messages/{messageId:guid}")]
    public async Task<ActionResult> Delete(Guid messageId)
    {
        await _service.DeleteAsync(User.GetUserId(), messageId);
        return NoContent();
    }

    [HttpPut("{conversationId:guid}/mute")]
    public async Task<ActionResult> Mute(Guid conversationId, MuteParticipantDto dto)
    {
        await _service.MuteAsync(User.GetUserId(), conversationId, dto.ParticipantId, dto.Duration);
        return NoContent();
    }

    [HttpDelete("{conversationId:guid}/mute/{participantId:guid}")]
    public async Task<ActionResult> Unmute(Guid conversationId, Guid participantId)
    {
        await _service.UnmuteAsync(User.GetUserId(), conversationId, participantId);
        return NoContent();
    }
}

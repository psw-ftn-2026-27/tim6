using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.Conversations;
using Social.Application.DirectConversationMessaging;

namespace Social.Api;

[ApiController]
[Route("api/social/conversations")]
[Authorize]
public sealed class DirectConversationMessagingController : ControllerBase
{
    private readonly DirectConversationMessagingService _service;
    private readonly DirectConversationMessagingQueries _queries;

    public DirectConversationMessagingController(
        DirectConversationMessagingService service, DirectConversationMessagingQueries queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpPost("direct/{otherUserId:guid}")]
    public async Task<ActionResult<Guid>> OpenDirect(Guid otherUserId) =>
        await _service.OpenDirectAsync(User.GetUserId(), otherUserId);

    [HttpGet("users")]
    public async Task<ActionResult<UserSearchPageDto>> SearchUsers(
        [FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        await _queries.SearchUsersAsync(searchTerm, page, pageSize);

    [HttpGet("mine")]
    public async Task<ActionResult<PageResult<ConversationSummaryDto>>> GetMine(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        await _queries.GetMineAsync(User.GetUserId(), page, pageSize);

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
}

namespace Social.Application.Blogs;

public sealed record CommentDto(
    Guid Id,
    Guid UserId,
    string? UserEmail,
    string Text,
    DateTime CreatedAt,
    DateTime LastModifiedAt,
    DateTime EditableUntil);

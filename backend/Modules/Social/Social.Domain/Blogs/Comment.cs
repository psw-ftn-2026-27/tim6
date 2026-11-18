using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Blogs;

public sealed class Comment : Entity
{
    public const int EditWindowMinutes = 15;

    public Guid UserId { get; private set; }
    public string Text { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModifiedAt { get; private set; }

    private Comment()
    {
        Text = null!;
    }

    public Comment(Guid userId, string text) : base(Guid.NewGuid())
    {
        EnsureValidText(text);

        UserId = userId;
        Text = text;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = CreatedAt;
    }

    public void EnsureEditableAt(DateTime now)
    {
        if (now - CreatedAt >= TimeSpan.FromMinutes(EditWindowMinutes))
        {
            throw new DomainException($"A comment can be changed only within {EditWindowMinutes} minutes of posting.");
        }
    }

    public void Update(string text, DateTime now)
    {
        EnsureEditableAt(now);
        EnsureValidText(text);

        Text = text;
        LastModifiedAt = now;
    }

    private static void EnsureValidText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A comment requires text.");
        }
    }
}

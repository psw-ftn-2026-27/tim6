using Shared.Domain.Exceptions;

namespace Shared.Domain.Conversations;

public sealed record MessageContent
{
    public string Value { get; private init; }

    private MessageContent()
    {
        Value = null!;
    }

    public MessageContent(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("A message requires content.");
        }

        Value = value;
    }

    public override string ToString() => Value;
}

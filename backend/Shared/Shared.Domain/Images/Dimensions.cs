using Shared.Domain.Exceptions;

namespace Shared.Domain.Images;

public sealed record Dimensions
{
    private const int MaxSideInPixels = 4096;

    public int Width { get; private init; }
    public int Height { get; private init; }

    private Dimensions() { }

    public Dimensions(int width, int height)
    {
        if (width > MaxSideInPixels || height > MaxSideInPixels)
        {
            throw new DomainException($"An image may be at most {MaxSideInPixels}x{MaxSideInPixels} pixels.");
        }

        Width = width;
        Height = height;
    }
}

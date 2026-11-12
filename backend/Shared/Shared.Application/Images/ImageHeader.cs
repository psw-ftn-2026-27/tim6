using Shared.Domain.Images;

namespace Shared.Application.Images;

public sealed record ImageHeader(ImageFormat Format, int Width, int Height);

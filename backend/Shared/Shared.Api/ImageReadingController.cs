using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ImageReading;

namespace Shared.Api;

[ApiController]
[Route("api/shared/images")]
[AllowAnonymous]
public sealed class ImageReadingController : ControllerBase
{
    private readonly ImageReadingQueries _readingQueries;

    public ImageReadingController(ImageReadingQueries readingQueries)
    {
        _readingQueries = readingQueries;
    }

    [HttpGet("{fileName}")]
    public async Task<ActionResult> Get(string fileName)
    {
        var image = await _readingQueries.GetAsync(fileName);
        return File(image.Content, image.ContentType);
    }
}

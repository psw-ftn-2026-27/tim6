using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ImageUploading;

namespace Shared.Api;

[ApiController]
[Route("api/shared/images")]
[Authorize]
public sealed class ImageUploadingController : ControllerBase
{
    private const int MaxRequestSizeInBytes = 5 * 1024 * 1024;

    private readonly ImageUploadingService _uploadingService;

    public ImageUploadingController(ImageUploadingService uploadingService)
    {
        _uploadingService = uploadingService;
    }

    [HttpPost]
    [RequestSizeLimit(MaxRequestSizeInBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestSizeInBytes)]
    public async Task<ActionResult<ImageReferenceDto>> Upload(IFormFile file)
    {
        using var content = file.OpenReadStream();
        return await _uploadingService.UploadAsync(User.GetUserId(), content);
    }

    [HttpDelete("{fileName}")]
    public async Task<ActionResult> Delete(string fileName)
    {
        await _uploadingService.DeleteAsync(fileName, User.GetUserId());
        return NoContent();
    }
}

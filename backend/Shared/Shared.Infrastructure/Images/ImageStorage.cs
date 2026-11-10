using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shared.Application.Images;

namespace Shared.Infrastructure.Images;

internal sealed class ImageStorage : IImageStorage
{
    private readonly string _directory;

    public ImageStorage(IConfiguration configuration, IHostEnvironment environment)
    {
        var directory = configuration["ImageStorage:Directory"];
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("The setting ImageStorage:Directory is not configured.");
        }

        _directory = Path.Combine(environment.ContentRootPath, directory);
    }

    public async Task SaveAsync(string fileName, byte[] content)
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllBytesAsync(PathOf(fileName), content);
    }

    public async Task<byte[]?> ReadAsync(string fileName)
    {
        var path = PathOf(fileName);
        return File.Exists(path) ? await File.ReadAllBytesAsync(path) : null;
    }

    public void Delete(string fileName)
    {
        File.Delete(PathOf(fileName));
    }

    private string PathOf(string fileName)
    {
        return Path.Combine(_directory, Path.GetFileName(fileName));
    }
}

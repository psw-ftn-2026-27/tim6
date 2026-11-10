namespace Shared.Application.Images;

public interface IImageStorage
{
    Task SaveAsync(string fileName, byte[] content);
    Task<byte[]?> ReadAsync(string fileName);
    void Delete(string fileName);
}

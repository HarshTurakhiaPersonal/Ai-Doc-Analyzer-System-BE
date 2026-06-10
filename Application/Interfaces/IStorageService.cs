namespace Application.Interfaces;

public interface IStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
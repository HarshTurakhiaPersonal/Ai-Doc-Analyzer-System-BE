namespace Application.Interfaces;

public interface IDocumentParserService
{
    Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default);
}

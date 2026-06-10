namespace Application.Interfaces;

public interface ISummaryService
{
    Task<string> GenerateSummaryAsync(string content, CancellationToken cancellationToken = default);
}
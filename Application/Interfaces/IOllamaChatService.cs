namespace Application.Interfaces;

public interface IOllamaChatService
{
    Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default);
}

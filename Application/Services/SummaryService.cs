using Application.Interfaces;

namespace Application.Services;

public sealed class SummaryService(IOllamaChatService chatService) : ISummaryService
{
    public async Task<string> GenerateSummaryAsync(string content, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
                    You are a professional document summarizer.

                    Generate a concise business summary in 5-10 sentences.

                    Document Content:

                    {content}

                    Summary:
                    """;

        return await chatService.AskAsync(prompt, cancellationToken);
    }
}
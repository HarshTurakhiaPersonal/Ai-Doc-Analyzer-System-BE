using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Application.Services;

public class RerankingService(IOllamaChatService chatService, ILogger<RerankingService> logger) : IRerankingService
{
    public async Task<List<DocumentChunk>> RerankAsync(string question, List<DocumentChunk> chunks)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        if (chunks.Count <= 5)
            return chunks;

        string prompt = BuildPrompt(question, chunks);

        string response = await chatService.AskAsync(prompt);

        List<int> ranking = ParseRanking(response);

        if (ranking.Count == 0)
            return chunks.Take(5).ToList();

        List<DocumentChunk> reranked = [];

        foreach (int index in ranking)
        {
            if (index - 1 < chunks.Count)
                reranked.Add(chunks[index - 1]);
        }

        stopwatch.Stop();
        logger.LogInformation("Reranking completed in {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);

        return reranked.Take(5).ToList();
    }

    private static string BuildPrompt(string question, List<DocumentChunk> chunks)
    {
        StringBuilder sb = new();

        sb.AppendLine("""
                        You are a document retrieval reranker.

                        Given a question and chunks.

                        Return ONLY the chunk numbers
                        ordered from most relevant
                        to least relevant.

                        Example:
                        3,1,5,2

                        No explanation.
                        """);

        sb.AppendLine();

        sb.AppendLine($"Question: {question}");

        sb.AppendLine();

        for (int i = 0; i < chunks.Count; i++)
        {
            sb.AppendLine(
                $"Chunk {i + 1}:");

            sb.AppendLine(
                chunks[i].ContentText);

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static List<int> ParseRanking(string response)
    {
        return response
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x =>
            {
                if (int.TryParse(
                    x.Trim(),
                    out int value))
                {
                    return value;
                }

                return -1;
            })
            .Where(x => x > 0)
            .Distinct()
            .ToList();
    }
}

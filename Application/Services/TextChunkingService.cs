using Application.Interfaces;
using System.Text;

namespace Application.Services;

public sealed class TextChunkingService : ITextChunkingService
{
    public IReadOnlyList<string> ChunkText(
        string text,
        int chunkSize = 1000,
        int overlap = 150)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        string normalizedText = text.Replace("\r\n", "\n");

        string[] paragraphs = normalizedText.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

        List<string> chunks = [];

        StringBuilder currentChunk = new();

        foreach (string paragraph in paragraphs)
        {
            string trimmedParagraph = paragraph.Trim();

            if (string.IsNullOrWhiteSpace(trimmedParagraph))
                continue;

            if (trimmedParagraph.Length > chunkSize)
            {
                FlushCurrentChunk(chunks, currentChunk);
                SplitLargeParagraph(trimmedParagraph, chunkSize, overlap, chunks);

                continue;
            }

            if (currentChunk.Length +
                trimmedParagraph.Length + 2 >
                chunkSize)
            {
                chunks.Add(currentChunk.ToString());

                string overlapText = GetOverlapText(currentChunk.ToString(), overlap);

                currentChunk.Clear();

                if (!string.IsNullOrWhiteSpace(overlapText))
                {
                    currentChunk.Append(overlapText);
                    currentChunk.AppendLine();
                    currentChunk.AppendLine();
                }
            }

            currentChunk.Append(trimmedParagraph);
            currentChunk.AppendLine();
            currentChunk.AppendLine();
        }

        FlushCurrentChunk(chunks, currentChunk);

        return chunks;
    }

    private static void FlushCurrentChunk(List<string> chunks, StringBuilder currentChunk)
    {
        if (currentChunk.Length == 0)
            return;


        chunks.Add(currentChunk.ToString().Trim());
        currentChunk.Clear();
    }

    private static string GetOverlapText(string text, int overlap)
    {
        if (text.Length <= overlap)
            return text;


        return text[^overlap..];
    }

    private static void SplitLargeParagraph(string paragraph, int chunkSize, int overlap, List<string> chunks)
    {
        int start = 0;

        while (start < paragraph.Length)
        {
            int length = Math.Min(chunkSize, paragraph.Length - start);

            chunks.Add(paragraph.Substring(start, length));

            start += chunkSize - overlap;
        }
    }
}
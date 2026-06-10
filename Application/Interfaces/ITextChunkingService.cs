namespace Application.Interfaces;

public interface ITextChunkingService
{
    IReadOnlyList<string> ChunkText(string text, int chunkSize = 500, int overlap = 100);
}
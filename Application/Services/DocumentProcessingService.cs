using Application.Interfaces;
using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.Interfaces.ICommon;
using Microsoft.Extensions.Logging;
using Pgvector;
using Shared.Enums;
using System.Diagnostics;

namespace Application.Services;

public sealed class DocumentProcessingService(
    IDocumentRepository documentRepository,
    IDocumentChunkRepository chunkRepository,
    IDocumentParserService parserService,
    ITextChunkingService chunkingService,
    IEmbeddingService embeddingService,
    ILogger<DocumentProcessingService> logger,
    IUnitOfWork unitOfWork)
    : IDocumentProcessingService
{
    public async Task ProcessDocumentAsync(Documents document, CancellationToken cancellationToken = default)
    {
        Stopwatch totalStopwatch = Stopwatch.StartNew();
        logger.LogInformation("Started processing document {DocumentId}", document.Id);

        document.Status = DocumentStatus.Processing;
        documentRepository.Update(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        Stopwatch extractionStopwatch = Stopwatch.StartNew();

        string text = await parserService.ExtractTextAsync(document.FilePath, cancellationToken);

        extractionStopwatch.Stop();
        logger.LogInformation("Text extraction completed in {ElapsedMs} ms", extractionStopwatch.ElapsedMilliseconds);

        Stopwatch chunkingStopwatch = Stopwatch.StartNew();

        IReadOnlyList<string> chunks = chunkingService.ChunkText(text);

        chunkingStopwatch.Stop();

        logger.LogInformation("Chunking completed in {ElapsedMs} ms. Generated {ChunkCount} chunks, DocumentId : {DocumentId}",
            chunkingStopwatch.ElapsedMilliseconds, chunks.Count, document.Id);

        DocumentChunk[] entities = new DocumentChunk[chunks.Count];

        Stopwatch embeddingStopwatch = Stopwatch.StartNew();

        await Parallel.ForEachAsync(
            chunks.Select((text, index) => new
            {
                Text = text,
                Index = index
            }),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = 4,
                CancellationToken = cancellationToken
            },
            async (chunk, ct) =>
            {
                try
                {
                    logger.LogInformation("Generating embedding for chunk {ChunkIndex}", chunk.Index + 1);
                    Vector embedding = await embeddingService.GenerateEmbeddingAsync(chunk.Text, ct);

                    entities[chunk.Index] =
                        new DocumentChunk
                        {
                            Id = Guid.NewGuid(),
                            DocumentId = document.Id,
                            ContentText = chunk.Text,
                            ChunkIndex = chunk.Index + 1,
                            Embedding = embedding
                        };

                    logger.LogInformation("Embedding generated for chunk {ChunkIndex}", chunk.Index + 1);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Embedding generation failed for chunk {ChunkIndex} of document {DocumentId}",
                        chunk.Index + 1,
                        document.Id);

                    throw;
                }
            });

        embeddingStopwatch.Stop();

        logger.LogInformation("Generated {ChunkCount} embeddings in {ElapsedMs} ms", chunks.Count, embeddingStopwatch.ElapsedMilliseconds);

        await chunkRepository.AddRangeAsync(entities.ToList());

        document.TotalChunks = entities.Length;
        document.Status = DocumentStatus.Processed;

        documentRepository.Update(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully processed document {DocumentId}. Total chunks: {ChunkCount}", document.Id, entities.Length);

        totalStopwatch.Stop();

        logger.LogInformation("Document {DocumentId} processed successfully in {ElapsedMs} ms", document.Id, totalStopwatch.ElapsedMilliseconds);
    }
}
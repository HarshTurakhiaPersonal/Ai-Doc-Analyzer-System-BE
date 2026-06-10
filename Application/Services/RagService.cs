using Application.DTOs.Response;
using Application.Interfaces;
using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.Configurations;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.ICommon;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Pgvector;
using Shared.Constants;
using Shared.Exceptions;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Application.Services;

public sealed class RagService(
    IOllamaChatService chatService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IDocumentQuestionRepository documentQuestionRepository,
    IEmbeddingService embeddingService,
    IRerankingService rerankingService,
    IDocumentAuthorizationService documentAuthorizationService,
    ILogger<RagService> logger,
    IChatMessageRepository chatMessageRepository,
    IDocumentChunkRepository chunkRepository) : IRagService
{
    public async Task<AskQuestionResponse> AskQuestionAsync(Guid documentId, string question, CancellationToken cancellationToken = default)
    {
        return await AskQuestionInternalAsync(
            documentId,
            question,
            string.Empty,
            cancellationToken);
    }

    public async Task<AskQuestionResponse> AskQuestionAsync(Guid documentId, string question, Guid? sessionId, CancellationToken cancellationToken = default)
    {
        string conversationContext = string.Empty;

        if (sessionId.HasValue)
        {
            List<ChatMessage> messages = await chatMessageRepository.GetRecentMessagesAsync(sessionId.Value, 6, cancellationToken);

            if (messages.Count > 0)
            {
                StringBuilder builder = new();

                builder.AppendLine("Conversation History:");

                foreach (ChatMessage message in messages)
                    builder.AppendLine($"{message.Role}: {message.Content}");

                builder.AppendLine();
                conversationContext = builder.ToString();
            }
        }

        return await AskQuestionInternalAsync(
            documentId,
            question,
            conversationContext,
            cancellationToken);
    }

    private async Task<AskQuestionResponse> AskQuestionInternalAsync(Guid documentId, string question, string conversationContext, CancellationToken cancellationToken = default)
    {
        Stopwatch totalStopwatch = Stopwatch.StartNew();
        Documents document = await documentAuthorizationService.GetOwnedDocumentAsync(documentId, cancellationToken);

        Stopwatch embeddingStopwatch = Stopwatch.StartNew();
        Vector queryEmbedding = await embeddingService.GenerateEmbeddingAsync(question, cancellationToken);

        embeddingStopwatch.Stop();
        logger.LogInformation("Query embedding generated in {ElapsedMs} ms", embeddingStopwatch.ElapsedMilliseconds);

        Stopwatch retrievalStopwatch = Stopwatch.StartNew();
        List<DocumentChunk> retrievedChunks = await chunkRepository.SearchSimilarChunksAsync(
                    documentId, queryEmbedding, 20, cancellationToken);

        retrievalStopwatch.Stop();
        logger.LogInformation("Retrieved {ChunkCount} chunks in {ElapsedMs} ms", retrievedChunks.Count, retrievalStopwatch.ElapsedMilliseconds);

        List<DocumentChunk> chunks = await rerankingService.RerankAsync(question, retrievedChunks);

        List<SourceReferenceResponse> sourceReferences =
            chunks
                .Select(x =>
                    new SourceReferenceResponse(
                        x.Id,
                        x.ChunkIndex))
                .ToList();

        string context = string.Join("\n\n", chunks.Select(x => x.ContentText));

        string prompt =
                        $$"""
                        You are an enterprise document assistant.

                        Use ONLY the supplied document context.

                        Conversation history may help understand references
                        such as "it", "they", "that service", etc.

                        Never invent information.

                        If the answer cannot be found in the document context,
                        respond exactly:

                        "I could not find that information in the document."

                        Conversation History:
                        {{conversationContext}}

                        Document Context:
                        {{context}}

                        Current Question:
                        {{question}}

                        Answer:
                        """;

        Stopwatch answerStopwatch = Stopwatch.StartNew();

        string answer = await chatService.AskAsync(prompt, cancellationToken);

        answerStopwatch.Stop();
        logger.LogInformation("Answer generated in {ElapsedMs} ms", answerStopwatch.ElapsedMilliseconds);

        string sourcesJson = JsonSerializer.Serialize(sourceReferences);

        DocumentQuestion questionEntity =
            new()
            {
                Id = Guid.NewGuid(),
                UserId = currentUserService.UserId,
                DocumentId = documentId,
                Question = question,
                Answer = answer,
                RetrievedChunksJson = sourcesJson,
                AskedAt = DateTime.UtcNow
            };

        await documentQuestionRepository.AddAsync(questionEntity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        totalStopwatch.Stop();
        logger.LogInformation("Question answered in {ElapsedMs} ms", totalStopwatch.ElapsedMilliseconds);

        return new AskQuestionResponse(
            answer,
            sourceReferences);
    }
}
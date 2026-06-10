using Application.DTOs.Response;
using Application.Interfaces;
using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.ICommon;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Enums;
using Shared.Exceptions;
using System.Diagnostics;
using System.Text.Json;

namespace Application.Services;

public sealed class DocumentService(
    IDocumentRepository documentRepository,
    IUnitOfWork unitOfWork,
    IStorageService storageService,
    IDocumentProcessingService processingService,
    IDocumentChunkRepository chunkRepository,
    ICurrentUserService currentUser,
    IDocumentQuestionRepository questionRepository,
    IDocumentAuthorizationService documentAuthorizationService,
    ILogger<DocumentService> logger,
    ISummaryService summaryService) : IDocumentService
{

    public async Task<Guid> UploadDocumentAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Documents? document = null;

        try
        {
            await using Stream stream = file.OpenReadStream();

            string filePath = await storageService.SaveFileAsync(stream, file.FileName, cancellationToken);

            if (!currentUser.IsAuthenticated)
                throw new UnauthorizedException(ApiResponseMessage.Authenticated_User);

            document = new()
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                FilePath = filePath,
                TotalChunks = 0,
                Status = DocumentStatus.Uploaded,
                UserId = currentUser.UserId
            };

            await documentRepository.AddAsync(document);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await processingService.ProcessDocumentAsync(document, cancellationToken);

            stopwatch.Stop();
            logger.LogInformation("Document {DocumentId} uploaded and processed successfully in {ElapsedMs} ms", document.Id, stopwatch.ElapsedMilliseconds);

            return document.Id;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Document upload failed after {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);

            if (document is not null)
            {
                document.Status = DocumentStatus.Failed;
                document.ErrorMessage = ex.Message;

                documentRepository.Update(document);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            throw;
        }
    }

    public async Task<DocumentSummaryResponse> GenerateSummaryAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        Documents? document = await documentAuthorizationService.GetOwnedDocumentAsync(documentId, cancellationToken);

        if (document is null)
            throw new NotFoundException(ApiResponseMessage.Doc_Not_Found);

        if (!string.IsNullOrWhiteSpace(document.Summary))
            return new DocumentSummaryResponse(document.Id, document.Summary);

        var chunks = await chunkRepository.GetByDocumentIdAsync(documentId, cancellationToken);

        if (!chunks.Any())
            throw new NotFoundException(ApiResponseMessage.Doc_Chunk_Not_Found);

        string content = string.Join(Environment.NewLine, chunks.Take(20).Select(x => x.ContentText));

        string summary = await summaryService.GenerateSummaryAsync(content, cancellationToken);

        document.Summary = summary;

        documentRepository.Update(document);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new DocumentSummaryResponse(document.Id, summary);
    }

    public async Task<DocumentSummaryResponse> GetSummaryAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        Documents? document = await documentAuthorizationService.GetOwnedDocumentAsync(documentId, cancellationToken);

        if (document is null)
            throw new NotFoundException(ApiResponseMessage.Doc_Not_Found);

        return new DocumentSummaryResponse(document.Id, document.Summary);
    }

    public async Task<List<QuestionHistoryResponse>> GetQuestionHistoryAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        Documents? document = await documentAuthorizationService.GetOwnedDocumentAsync(documentId, cancellationToken);

        if (document is null)
            throw new NotFoundException(ApiResponseMessage.Doc_Not_Found);

        List<DocumentQuestion> questions = await questionRepository.GetByDocumentIdAsync(documentId, currentUser.UserId, cancellationToken);

        return questions.Select(x =>
        {
            var references = JsonSerializer.Deserialize<List<SourceReferenceResponse>>(x.RetrievedChunksJson) ?? [];

            return new QuestionHistoryResponse(
                x.Id,
                x.Question,
                x.Answer,
                references,
                x.AskedAt);
        }).ToList();
    }
}
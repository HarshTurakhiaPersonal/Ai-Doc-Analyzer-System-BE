using Application.DTOs.Response;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IDocumentService
{
    Task<Guid> UploadDocumentAsync(IFormFile file, CancellationToken cancellationToken = default);

    Task<DocumentSummaryResponse> GenerateSummaryAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task<DocumentSummaryResponse> GetSummaryAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<List<QuestionHistoryResponse>> GetQuestionHistoryAsync(Guid documentId, CancellationToken cancellationToken = default);

}
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Shared.Constants;

namespace WebApi.Endpoints;

public static class DocumentEndpoints
{
    private const string BaseRoute = "/api/documents";

    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Documents")
            .RequireAuthorization();

        group.MapPost("/", UploadDocument)
            .WithName(nameof(UploadDocument))
            .DisableAntiforgery()
            .Produces<ApiResponse<UploadDocumentResponse>>(StatusCodes.Status201Created);

        group.MapPost("/{id:guid}/ask", AskDocumentQuestion)
            .WithName(nameof(AskDocumentQuestion))
            .Produces<ApiResponse<AskQuestionResponse>>();

        group.MapPost("/{id:guid}/summary", GenerateSummary)
            .WithName(nameof(GenerateSummary))
            .Produces<ApiResponse<DocumentSummaryResponse>>();

        group.MapGet("/{id:guid}/summary", GetSummary)
            .WithName(nameof(GetSummary))
            .Produces<ApiResponse<DocumentSummaryResponse>>();

        group.MapGet("/{id:guid}/questions", GetQuestionHistory)
            .WithName(nameof(GetQuestionHistory))
            .Produces<ApiResponse<List<QuestionHistoryResponse>>>();

        return app;
    }

    private static async Task<ApiResponse<UploadDocumentResponse>> UploadDocument(
        IFormFile file,
        IDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var documentId = await documentService.UploadDocumentAsync(file, cancellationToken);

        return new(
            new UploadDocumentResponse(documentId, file.FileName),
            ApiResponseMessage.Document_Uploaded,
            StatusCodes.Status201Created);
    }

    private static async Task<ApiResponse<AskQuestionResponse>> AskDocumentQuestion(
        Guid id,
        AskDocumentRequest request,
        IRagService ragService,
        CancellationToken cancellationToken)
    {
        var response = await ragService.AskQuestionAsync(
            id,
            request.Question
            );

        return new(
            response,
            ApiResponseMessage.Question_Answered_Success,
            StatusCodes.Status200OK);
    }

    private static async Task<ApiResponse<DocumentSummaryResponse>> GenerateSummary(
        Guid id,
        IDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var summary = await documentService.GenerateSummaryAsync(id, cancellationToken);

        return new(
            summary,
            ApiResponseMessage.Summary_Generated,
            StatusCodes.Status200OK);
    }

    private static async Task<ApiResponse<DocumentSummaryResponse>> GetSummary(
        Guid id,
        IDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var summary = await documentService.GetSummaryAsync(id, cancellationToken);

        return new(
            summary,
            ApiResponseMessage.Summary_Retrieval,
            StatusCodes.Status200OK);
    }

    private static async Task<ApiResponse<List<QuestionHistoryResponse>>> GetQuestionHistory(
        Guid id,
        IDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var history = await documentService.GetQuestionHistoryAsync(
            id,
            cancellationToken);

        return new(
            history,
            ApiResponseMessage.Question_Retrival_Success,
            StatusCodes.Status200OK);
    }

}
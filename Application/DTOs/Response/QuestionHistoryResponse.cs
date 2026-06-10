namespace Application.DTOs.Response;

public sealed record QuestionHistoryResponse(
    Guid Id,
    string Question,
    string Answer,
    List<SourceReferenceResponse> Sources,
    DateTime AskedAt);
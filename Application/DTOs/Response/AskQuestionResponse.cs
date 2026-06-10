namespace Application.DTOs.Response;

public sealed record AskQuestionResponse(string Answer, List<SourceReferenceResponse> Sources);
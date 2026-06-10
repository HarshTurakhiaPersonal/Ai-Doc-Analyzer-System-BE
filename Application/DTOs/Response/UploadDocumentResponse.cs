namespace Application.DTOs.Response;

public sealed record UploadDocumentResponse(Guid DocumentId, string FileName);
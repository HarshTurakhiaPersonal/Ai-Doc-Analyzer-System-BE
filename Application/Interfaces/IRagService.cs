using Application.DTOs.Response;

namespace Application.Interfaces;

public interface IRagService
{
    Task<AskQuestionResponse> AskQuestionAsync(
        Guid documentId,
        string question,
        Guid? sessionId,
        CancellationToken cancellationToken = default);

    Task<AskQuestionResponse> AskQuestionAsync(
    Guid documentId,
    string question,
    CancellationToken cancellationToken = default);

}
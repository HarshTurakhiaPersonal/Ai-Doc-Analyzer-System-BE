using Application.DTOs.Request;
using Application.DTOs.Response;

namespace Application.Interfaces;

public interface IChatSessionService
{
    Task<Guid> CreateSessionAsync(CreateChatSessionRequest request, CancellationToken cancellationToken = default);

    Task<List<ChatSessionResponse>> GetSessionsAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task<List<ChatMessageResponse>> GetMessagesAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<AskQuestionResponse> SendMessageAsync(Guid sessionId, string message, CancellationToken cancellationToken = default);

    Task DeleteSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

}
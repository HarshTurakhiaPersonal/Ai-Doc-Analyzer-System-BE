using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Configurations;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.ICommon;
using Shared.Constants;
using Shared.Enums;
using Shared.Exceptions;
using System.Text;

namespace Application.Services;

public sealed class ChatSessionService(
    IChatSessionRepository chatSessionRepository,
    IChatMessageRepository chatMessageRepository,
    IDocumentAuthorizationService documentAuthorizationService,
    ICurrentUserService currentUser,
    IRagService ragService,
    IUnitOfWork unitOfWork)
    : IChatSessionService
{
    public async Task<Guid> CreateSessionAsync(CreateChatSessionRequest request, CancellationToken cancellationToken = default)
    {
        Documents document = await documentAuthorizationService.GetOwnedDocumentAsync(request.DocumentId, cancellationToken);

        ChatSession session = new()
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId,
            DocumentId = document.Id,
            Title = string.IsNullOrWhiteSpace(
                        request.Title)
                    ? document.FileName
                    : request.Title,
            LastMessageAt = DateTime.UtcNow
        };

        await chatSessionRepository.AddAsync(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return session.Id;
    }

    public async Task<List<ChatSessionResponse>> GetSessionsAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        await documentAuthorizationService.GetOwnedDocumentAsync(documentId, cancellationToken);

        List<ChatSession> sessions = await chatSessionRepository.GetByDocumentIdAsync(documentId, currentUser.UserId, cancellationToken);

        return sessions
            .Select(x =>
                new ChatSessionResponse(
                    x.Id,
                    x.DocumentId,
                    x.Title,
                    x.LastMessageAt))
            .ToList();
    }

    public async Task<List<ChatMessageResponse>> GetMessagesAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        ChatSession? session = await chatSessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session is null)
            throw new NotFoundException(ApiResponseMessage.Chat_Not_Found);

        if (session.UserId != currentUser.UserId)
            throw new UnauthorizedException(ApiResponseMessage.Authenticated_User);

        List<ChatMessage> messages = await chatMessageRepository.GetBySessionIdAsync(sessionId, cancellationToken);

        return messages.Select(x => new ChatMessageResponse(x.Id, x.Role, x.Content, x.CreatedAt))
            .ToList();
    }

    public async Task<AskQuestionResponse> SendMessageAsync(Guid sessionId, string message, CancellationToken cancellationToken = default)
    {
        ChatSession? session = await chatSessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session is null)
        {
            throw new NotFoundException(ApiResponseMessage.Chat_Not_Found);
        }

        if (session.UserId != currentUser.UserId)
        {
            throw new UnauthorizedException(ApiResponseMessage.Authenticated_User);
        }

        AskQuestionResponse response = await ragService.AskQuestionAsync(session.DocumentId, message, sessionId, cancellationToken);

        await chatMessageRepository.AddAsync(
            new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Role = ChatRoles.User,
                Content = message,
                TokenCount = message.Length / 4
            });

        await chatMessageRepository.AddAsync(
            new ChatMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Role = ChatRoles.Assistant,
                Content = response.Answer,
                TokenCount = response.Answer.Length / 4
            });

        session.LastMessageAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task DeleteSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        ChatSession? session = await chatSessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session is null)
            throw new NotFoundException(ApiResponseMessage.Chat_Not_Found);

        if (session.UserId != currentUser.UserId)
            throw new UnauthorizedException(ApiResponseMessage.Authenticated_User);

        chatSessionRepository.Remove(session);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

}
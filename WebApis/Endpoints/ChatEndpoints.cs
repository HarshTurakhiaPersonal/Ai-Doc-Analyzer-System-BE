using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Shared.Constants;

namespace WebApi.Endpoints;

public static class ChatEndpoints
{
    private const string BaseRoute = "/api/chat";

    public static IEndpointRouteBuilder MapChatEndpoints(
        this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(BaseRoute)
            .WithTags("Chat")
            .RequireAuthorization();

        group.MapPost("/sessions", CreateSession);

        group.MapGet("/documents/{documentId:guid}/sessions", GetSessions);

        group.MapGet("/sessions/{sessionId:guid}/messages", GetMessages);

        group.MapPost("/sessions/{sessionId:guid}/messages", SendMessage);

        group.MapDelete("/sessions/{sessionId:guid}", DeleteSession);

        return app;
    }

    private static async Task<IResult> CreateSession(
        CreateChatSessionRequest request,
        IChatSessionService service,
        CancellationToken cancellationToken)
    {
        Guid sessionId =
            await service.CreateSessionAsync(
                request,
                cancellationToken);

        return Results.Ok(
            new
            {
                SessionId = sessionId
            });
    }

    private static async Task<IResult> GetSessions(
        Guid documentId,
        IChatSessionService service,
        CancellationToken cancellationToken)
    {
        List<ChatSessionResponse> sessions =
            await service.GetSessionsAsync(
                documentId,
                cancellationToken);

        return Results.Ok(sessions);
    }

    private static async Task<IResult> GetMessages(
        Guid sessionId,
        IChatSessionService service,
        CancellationToken cancellationToken)
    {
        List<ChatMessageResponse> messages =
            await service.GetMessagesAsync(
                sessionId,
                cancellationToken);

        return Results.Ok(messages);
    }

    private static async Task<IResult> SendMessage(
        Guid sessionId,
        ChatMessageRequest request,
        IChatSessionService service,
        CancellationToken cancellationToken)
    {
        AskQuestionResponse response =
            await service.SendMessageAsync(
                sessionId,
                request.Message,
                cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteSession(
        Guid sessionId,
        IChatSessionService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteSessionAsync(
            sessionId,
            cancellationToken);

        return Results.NoContent();
    }
}
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Shared.Constants;
using System.Net;
using System.Security.Claims;

namespace WebApi.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        app.MapGet("/me", (ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated,
                Name = user.Identity?.Name,
                Claims = user.Claims.Select(c => new { c.Type, c.Value })
            });
        })
        .RequireAuthorization();

        group.MapPost("/register", Register)
            .WithName("RegisterUser")
            .Accepts<RegisterRequest>("application/json")
            .Produces<ApiResponse<RegisterUserResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<RegisterUserResponse>>(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();

        group.MapPost("/login", Login)
            .WithName("LoginUser")
            .Accepts<LoginRequest>("application/json")
            .Produces<ApiResponse<UserLogInResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UserLogInResponse>>(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();

        group.MapPost("/logout", Logout)
            .RequireAuthorization();

        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .Accepts<RefreshTokenRequest>("application/json")
            .Produces<ApiResponse<UserLogInResponse>>(StatusCodes.Status200OK)
            .DisableAntiforgery();

        return app;
    }

    private static async Task<ApiResponse<RegisterUserResponse>> Register(RegisterRequest request, IAuthService authService)
    {
        return new ApiResponse<RegisterUserResponse>(
            await authService.RegisterAsync(request),
            ApiResponseMessage.User_Registered,
            StatusCodes.Status201Created);
    }

    private static async Task<ApiResponse<UserLogInResponse>> Login(LoginRequest request, IAuthService authService)
    {
        return new ApiResponse<UserLogInResponse>(
            await authService.LoginAsync(request),
            ApiResponseMessage.User_LoggedIn,
            StatusCodes.Status200OK);
    }

    private static async Task<IResult> Logout(LogoutRequest request, IAuthService authService, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return Results.Ok();
    }

    private static async Task<ApiResponse<UserLogInResponse>> Refresh(RefreshTokenRequest request, IAuthService authService, CancellationToken cancellationToken)
    {
        return new ApiResponse<UserLogInResponse>(
            await authService.RefreshAsync(request.RefreshToken, cancellationToken),
            ApiResponseMessage.Token_Refreshed,
            StatusCodes.Status200OK);
    }
}
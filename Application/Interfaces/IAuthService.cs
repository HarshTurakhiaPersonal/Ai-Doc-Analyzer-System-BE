using Application.DTOs.Request;
using Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<RegisterUserResponse> RegisterAsync(RegisterRequest request);
    Task<UserLogInResponse> LoginAsync(LoginRequest request);
    Task<UserLogInResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
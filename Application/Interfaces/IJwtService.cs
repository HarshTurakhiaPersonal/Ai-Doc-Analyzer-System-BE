using Application.DTOs.Response;
using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtService
{
    Task<UserLogInResponse> GenerateTokenAsync(ApplicationUser user);
    Task<string> GenerateRefreshTokenAsync();
}
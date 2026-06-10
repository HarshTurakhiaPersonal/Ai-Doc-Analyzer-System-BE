using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Shared.Constants;
using Shared.Exceptions;
using System.Security.Claims;

namespace Application.Services;

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated
        ?? false;

    public Guid UserId
    {
        get
        {
            string? userId =
                httpContextAccessor.HttpContext?
                    .User?
                    .FindFirstValue(
                        ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out Guid id))
            {
                throw new UnauthorizedException(
                    ApiResponseMessage.Authenticated_User);
            }

            return id;
        }
    }

    public string Email
    {
        get
        {
            string? email =
                httpContextAccessor.HttpContext?
                    .User?
                    .FindFirstValue(
                        ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new UnauthorizedException(
                    ApiResponseMessage.Authenticated_User);
            }

            return email;
        }
    }

    public string Role
    {
        get
        {
            string? role =
                httpContextAccessor.HttpContext?
                    .User?
                    .FindFirstValue(
                        ClaimTypes.Role);

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new UnauthorizedException(
                    ApiResponseMessage.Authenticated_User);
            }

            return role;
        }
    }
}
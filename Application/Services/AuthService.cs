using Application.Common;
using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.ICommon;
using Microsoft.AspNetCore.Identity;
using Shared.Constants;
using Shared.Enums;
using Shared.Exceptions;

namespace Application.Services.Auth;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<RegisterUserResponse> RegisterAsync(RegisterRequest request)
    {
        bool exists = await userManager.FindByEmailAsync(request.Email) is not null;

        if (exists)
            throw new BadRequestException(ApiResponseMessage.Email_Exists);

        ApplicationUser user = new()
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

        await userManager.AddToRoleAsync(user, Roles.User);
        var roles = await userManager.GetRolesAsync(user);

        UserDto userDto = new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = roles,
        };

        return new RegisterUserResponse(userDto);
    }

    public async Task<UserLogInResponse> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            throw new UnauthorizedException(ApiResponseMessage.Invalid_Creds);

        var signInResult = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
            throw new UnauthorizedException(ApiResponseMessage.Account_Locked);

        if (!signInResult.Succeeded)
            throw new UnauthorizedException(ApiResponseMessage.Invalid_Creds);

        UserLogInResponse response = await jwtService.GenerateTokenAsync(user);

        RefreshToken token = new()
        {
            UserId = user.Id,
            TokenHash = SecurityHelper.ComputeSha256Hash(response.RefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        await refreshTokenRepository.AddAsync(token);
        await unitOfWork.SaveChangesAsync();

        return response;

    }

    public async Task<UserLogInResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string hash = SecurityHelper.ComputeSha256Hash(refreshToken);

        RefreshToken? token =
            await refreshTokenRepository
                .GetByTokenHashAsync(hash, cancellationToken);

        if (token is null)
        {
            throw new UnauthorizedException(ApiResponseMessage.Invalid_Token);
        }

        if (token.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedException(ApiResponseMessage.Token_Expired);
        }

        if (token.IsRevoked)
        {
            List<RefreshToken> activeTokens = await refreshTokenRepository.GetActiveTokensByUserIdAsync(token.UserId, cancellationToken);

            foreach (RefreshToken active in activeTokens)
            {
                active.IsRevoked = true;
                active.RevokedAt = DateTime.UtcNow;
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException(ApiResponseMessage.Refresh_Token_Reuse);
        }

        string newRefreshToken = await jwtService.GenerateRefreshTokenAsync();

        string newHash = SecurityHelper.ComputeSha256Hash(newRefreshToken);

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByTokenHash = newHash;

        await refreshTokenRepository.AddAsync(
            new RefreshToken
            {
                UserId = token.UserId,
                TokenHash = newHash,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });

        UserLogInResponse jwt = await jwtService.GenerateTokenAsync(token.User);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return jwt with
        {
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        string hash = SecurityHelper.ComputeSha256Hash(refreshToken);

        RefreshToken? token = await refreshTokenRepository.GetByTokenHashAsync(hash, cancellationToken);

        if (token is null)
            return;

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

}
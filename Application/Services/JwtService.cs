using Application.DTOs.Response;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public sealed class JwtService(UserManager<ApplicationUser> userManager, IOptions<JwtOptions> options) : IJwtService
{
    public async Task<UserLogInResponse> GenerateTokenAsync(ApplicationUser user)
    {
        var jwtOptions = options.Value;

        var roles = await userManager.GetRolesAsync(user);

        string jwtId = Guid.NewGuid().ToString();

        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Email,user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti,jwtId)
        ];

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes);

        JwtSecurityToken token = new(
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        string refreshToken = await GenerateRefreshTokenAsync();

        return new UserLogInResponse(accessToken, refreshToken, expiresAt);
    }

    public Task<string> GenerateRefreshTokenAsync()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);

        return Task.FromResult(Convert.ToBase64String(bytes));
    }
}
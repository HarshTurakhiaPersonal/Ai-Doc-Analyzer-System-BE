namespace Application.DTOs.Response;

public sealed record UserLogInResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public sealed record RegisterUserResponse(UserDto user);
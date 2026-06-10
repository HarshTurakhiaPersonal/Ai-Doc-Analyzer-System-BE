namespace Application.DTOs.Response;

public sealed record UserSearchResponse(IReadOnlyCollection<string> Matches);
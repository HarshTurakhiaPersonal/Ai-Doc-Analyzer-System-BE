namespace Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string JwtId { get; set; } = string.Empty;
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByTokenHash { get; set; }
}
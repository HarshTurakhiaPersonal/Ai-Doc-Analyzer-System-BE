namespace Domain.Entities;

public sealed class DocumentQuestion : BaseEntity
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime AskedAt { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Documents Document { get; set; } = null!;
    public string RetrievedChunksJson { get; set; } = string.Empty;
}
using Shared.Enums;

namespace Domain.Entities;

public sealed class Documents : BaseEntity
{
    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public int TotalChunks { get; set; }

    public DocumentStatus Status { get; set; }

    public string? ErrorMessage { get; set; } = string.Empty;

    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();

    public string Summary { get; set; } = string.Empty;

    public DateTime? SummaryGeneratedAt { get; set; }

    public bool IsSummaryGenerated { get; set; }

    public Guid? UserId { get; set; }

    public ApplicationUser? User { get; set; } = null!;

    public ICollection<ChatSession> ChatSessions { get; set; } = [];
}
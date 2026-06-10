namespace Domain.Entities;

public sealed class ChatSession : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Documents Document { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = [];
}
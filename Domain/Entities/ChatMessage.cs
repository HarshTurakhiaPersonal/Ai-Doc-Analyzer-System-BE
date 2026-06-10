namespace Domain.Entities;

public sealed class ChatMessage : BaseEntity
{
    public Guid SessionId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public ChatSession Session { get; set; } = null!;
}
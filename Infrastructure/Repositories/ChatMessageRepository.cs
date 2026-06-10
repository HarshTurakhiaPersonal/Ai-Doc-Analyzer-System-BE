using Domain.Entities;
using Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ChatMessageRepository(AppDbContext context) : GenericRepository<ChatMessage>(context), IChatMessageRepository
{
    public async Task<List<ChatMessage>> GetRecentMessagesAsync(Guid sessionId, int count, CancellationToken cancellationToken = default)
    {
        return await context.ChatMessages
            .Where(x => x.SessionId == sessionId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChatMessage>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.ChatMessages
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
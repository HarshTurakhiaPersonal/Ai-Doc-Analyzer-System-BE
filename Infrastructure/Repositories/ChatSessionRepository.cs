using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ChatSessionRepository(AppDbContext context) : GenericRepository<ChatSession>(context), IChatSessionRepository
{
    public async Task<ChatSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.ChatSessions
            .Include(x => x.Document)
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
    }

    public async Task<List<ChatSession>> GetByDocumentIdAsync(Guid documentId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.ChatSessions
            .Where(x => x.DocumentId == documentId && x.UserId == userId)
            .OrderByDescending(x => x.LastMessageAt)
            .ToListAsync(cancellationToken);
    }
}
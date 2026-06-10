using Application.Interfaces.IRepositories;
using Domain.Entities;

namespace Infrastructure.Interfaces;

public interface IChatSessionRepository : IGenericRepository<ChatSession>
{
    Task<ChatSession?> GetByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<List<ChatSession>> GetByDocumentIdAsync(
        Guid documentId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

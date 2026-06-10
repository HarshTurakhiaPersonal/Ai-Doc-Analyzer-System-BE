using Domain.Entities;

namespace Application.Interfaces.IRepositories;

public interface IDocumentRepository
{
    Task<Documents?> GetByIdAsync(Guid id);

    Task<Documents?> GetWithChunksAsync(Guid id);

    Task AddAsync(Documents document);

    void Update(Documents document);

    Task<bool> ExistsAsync(Guid id);

    Task<Documents?> GetByIdAndUserIdAsync(Guid documentId, Guid userId, CancellationToken cancellationToken);
}
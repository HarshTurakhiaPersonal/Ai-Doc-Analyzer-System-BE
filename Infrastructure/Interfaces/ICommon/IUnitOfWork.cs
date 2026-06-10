using Application.Interfaces.IRepositories;

namespace Infrastructure.Interfaces.ICommon;

public interface IUnitOfWork
{
    IDocumentRepository Documents { get; }

    IDocumentChunkRepository DocumentChunks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
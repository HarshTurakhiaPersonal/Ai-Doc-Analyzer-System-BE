using Application.Interfaces.IRepositories;
using Infrastructure.Interfaces.ICommon;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories.Common;

public sealed class UnitOfWork(AppDbContext context, IDocumentRepository documentRepository, IDocumentChunkRepository documentChunkRepository) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IDocumentRepository Documents => documentRepository;

    public IDocumentChunkRepository DocumentChunks => documentChunkRepository;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
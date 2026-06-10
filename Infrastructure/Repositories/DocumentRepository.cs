using Application.Interfaces.IRepositories;
using Domain.Entities;
using System.Data.Entity;

namespace Infrastructure.Repositories;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Documents?> GetByIdAsync(Guid id)
    {
        return await _context.Documents.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Documents?> GetWithChunksAsync(Guid id)
    {
        return await _context.Documents
            .Include(x => x.Chunks).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Documents document)
    {
        await _context.Documents.AddAsync(document);
    }

    public void Update(Documents document)
    {
        _context.Documents.Update(document);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Documents.AnyAsync(x => x.Id == id);
    }

    public async Task<Documents?> GetByIdAndUserIdAsync(Guid documentId, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Documents.FirstOrDefaultAsync(x => x.Id == documentId && x.UserId == userId, cancellationToken);
    }
}
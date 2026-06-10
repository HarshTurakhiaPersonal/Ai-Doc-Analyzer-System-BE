using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DocumentQuestionRepository(AppDbContext context) : GenericRepository<DocumentQuestion>(context), IDocumentQuestionRepository
{
    private readonly AppDbContext _context = context;

    public async Task<List<DocumentQuestion>> GetByDocumentIdAsync(Guid documentId, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.DocumentQuestions
            .Where(x => x.DocumentId == documentId && x.UserId == userId)
            .OrderByDescending(x => x.AskedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentQuestion?> GetByIdAsync(Guid questionId, CancellationToken cancellationToken)
    {
        return await context.DocumentQuestions.FirstOrDefaultAsync(x => x.Id == questionId, cancellationToken);
    }
}

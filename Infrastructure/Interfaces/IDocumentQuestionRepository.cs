using Application.Interfaces.IRepositories;
using Domain.Entities;

namespace Infrastructure.Interfaces;

public interface IDocumentQuestionRepository : IGenericRepository<DocumentQuestion>
{
    Task<List<DocumentQuestion>> GetByDocumentIdAsync(Guid documentId, Guid userId, CancellationToken cancellationToken);
    Task<DocumentQuestion?> GetByIdAsync(Guid questionId, CancellationToken cancellationToken);
}
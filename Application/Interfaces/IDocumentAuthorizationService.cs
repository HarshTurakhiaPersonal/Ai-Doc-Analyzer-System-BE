using Domain.Entities;

namespace Application.Interfaces;

public interface IDocumentAuthorizationService
{
    Task<Documents> GetOwnedDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
}
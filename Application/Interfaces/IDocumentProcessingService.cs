using Domain.Entities;

namespace Application.Interfaces;

public interface IDocumentProcessingService
{
    Task ProcessDocumentAsync(Documents document, CancellationToken cancellationToken = default);
}
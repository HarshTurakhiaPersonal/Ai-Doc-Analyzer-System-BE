using Application.Interfaces;
using Application.Interfaces.IRepositories;
using Domain.Entities;
using Shared.Constants;
using Shared.Exceptions;

namespace Application.Services;

public sealed class DocumentAuthorizationService(
    IDocumentRepository documentRepository,
    ICurrentUserService currentUser)
    : IDocumentAuthorizationService
{
    public async Task<Documents> GetOwnedDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        Documents? document = await documentRepository.GetByIdAndUserIdAsync(documentId, currentUser.UserId, cancellationToken);

        if (document is null)
            throw new NotFoundException(ApiResponseMessage.Doc_Not_Found);

        return document;
    }
}
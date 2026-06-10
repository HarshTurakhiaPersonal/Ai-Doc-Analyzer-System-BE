namespace Shared.Exceptions;

public sealed class DocumentNotFoundException : Exception
{
    public DocumentNotFoundException(Guid documentId) : base($"Document '{documentId}' not found")
    {
    }
}
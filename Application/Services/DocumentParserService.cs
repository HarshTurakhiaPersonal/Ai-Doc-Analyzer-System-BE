using Application.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using UglyToad.PdfPig;

namespace Application.Services;

public sealed class DocumentParserService() : IDocumentParserService
{
    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => await ParsePdfAsync(filePath, cancellationToken),

            ".docx" => await ParseDocAsync(filePath, cancellationToken),

            ".txt" => await ParseTextAsync(filePath, cancellationToken),

            _ => throw new NotSupportedException($"Unsupported file type: {extension}")
        };
    }

    private Task<string> ParsePdfAsync(string filePath, CancellationToken cancellationToken = default)
    {
        StringBuilder builder = new();

        using PdfDocument document = PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
            builder.AppendLine(page.Text);

        return Task.FromResult(builder.ToString());
    }

    private Task<string> ParseDocAsync(string filePath, CancellationToken cancellationToken = default)
    {
        StringBuilder builder = new();

        using WordprocessingDocument document = WordprocessingDocument.Open(filePath, false);

        var body = document.MainDocumentPart?.Document.Body;

        if (body is not null)
            builder.Append(body.InnerText);

        return Task.FromResult(builder.ToString());
    }

    private async Task<string> ParseTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(filePath, cancellationToken);
    }
}
using Application.Interfaces;
using System.Text.RegularExpressions;

namespace Application.Services;

public class StorageService : IStorageService
{
    private readonly string _uploadRoot;

    public StorageService()
    {
        _uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(_uploadRoot);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        string yearFolder = now.Year.ToString();
        string monthFolder = now.Month.ToString("00");
        string dayFolder = now.Day.ToString("00");

        string folderPath = Path.Combine(_uploadRoot, yearFolder, monthFolder, dayFolder);

        Directory.CreateDirectory(folderPath);

        string extension = Path.GetExtension(fileName);

        string originalName = Path.GetFileNameWithoutExtension(fileName);

        originalName = SanitizeFileName(originalName);

        string timestamp = now.ToString("yyyyMMdd_HHmmss");

        string finalFileName = $"{originalName}_{timestamp}_{Random.Shared.Next(1000, 9999)}{extension}";

        string fullPath = Path.Combine(folderPath, finalFileName);

        await using FileStream outputStream = new(fullPath,
                                             FileMode.Create,
                                             FileAccess.Write,
                                             FileShare.None);

        await fileStream.CopyToAsync(outputStream, cancellationToken);
        return fullPath;
    }

    private static string SanitizeFileName(string fileName)
    {
        fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9_\- ]", "");

        fileName = fileName.Replace(" ", "_");

        return fileName.Length > 100
            ? fileName[..100]
            : fileName;
    }
}

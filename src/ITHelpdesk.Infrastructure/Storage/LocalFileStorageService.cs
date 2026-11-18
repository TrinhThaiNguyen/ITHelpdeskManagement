
using ITHelpdesk.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ITHelpdesk.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public LocalFileStorageService(IHostEnvironment environment)
    {
        _storagePath = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "TicketAttachments");

        Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var storageKey = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";

        var filePath = GetSafeFilePath(storageKey);

        await using var outputStream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true);

        await fileStream.CopyToAsync(
            outputStream,
            cancellationToken);

        return storageKey;
    }

    public async Task DeleteFileAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var filePath = GetSafeFilePath(storageKey);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        await Task.CompletedTask;
    }

    public Task<Stream?> GetFileAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var filePath = GetSafeFilePath(storageKey);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            useAsync: true);

        return Task.FromResult<Stream?>(stream);
    }

    private string GetSafeFilePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) ||
            storageKey != Path.GetFileName(storageKey))
        {
            throw new ArgumentException("Invalid storage key.");
        }

        return Path.Combine(_storagePath, storageKey);
    }
}

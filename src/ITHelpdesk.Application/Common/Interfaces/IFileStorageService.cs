
namespace ITHelpdesk.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task<Stream?> GetFileAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}

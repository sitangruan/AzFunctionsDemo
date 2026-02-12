using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FuncUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlobTimerFunc.Services;
public sealed class BlobStorageAdapter : IBlobStorageAdapter
{
    private readonly BlobServiceClient _client;
    private readonly ILogger<BlobStorageAdapter> _logger;

    public BlobStorageAdapter(IOptions<BlobSettings> settings, ILogger<BlobStorageAdapter> logger)
    {
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        var conn = settings?.Value?.ConnectionString ?? throw new System.ArgumentNullException(nameof(settings));
        if (string.IsNullOrWhiteSpace(conn))
        {
            _logger.LogWarning("Blob connection string is empty; calls may fail.");
        }

        _client = new BlobServiceClient(conn);
    }

    public async Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(containerName);
        return await container.ExistsAsync(cancellationToken).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<string> ListBlobNamesAsync(string containerName, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(containerName);
        await foreach (BlobItem item in container.GetBlobsAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            yield return item.Name;
        }
    }
}
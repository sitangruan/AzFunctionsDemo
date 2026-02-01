using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlobTimerFunc.Services;
public sealed class BlobLister : IBlobLister
{
    private readonly BlobSettings _settings;
    private readonly IBlobStorageAdapter _adapter;
    private readonly ILogger<BlobLister> _logger;

    public BlobLister(IOptions<BlobSettings> settings, IBlobStorageAdapter adapter, ILogger<BlobLister> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Count blobs. Supports:
    /// - fast adapter enumeration when no limits/continuation are provided.
    /// - paged enumeration using SDK AsPages when maxItems or continuationToken are specified.
    /// Returns (Count, IsTruncated, ContinuationToken).
    /// </summary>
    public async Task<(long Count, bool IsTruncated, string? ContinuationToken)> CountBlobsAsync(
        string? containerName = null,
        long maxItems = long.MaxValue,
        int pageSize = 500,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var name = containerName ?? _settings.ContainerName;

        // If no paging/limit requested, prefer adapter simple enumeration (keeps adapter useful for unit tests)
        if (maxItems == long.MaxValue && string.IsNullOrEmpty(continuationToken))
        {
            if (!await _adapter.ContainerExistsAsync(name, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogWarning("The container '{ContainerName}' does not exist.", name);
                return (0L, false, null);
            }

            long count = 0;
            await foreach (var _ in _adapter.ListBlobNamesAsync(name, cancellationToken).ConfigureAwait(false))
            {
                count++;
                if ((count % 10000) == 0)
                {
                    _logger.LogInformation("Enumerated {Count} blobs so far in container '{ContainerName}'.", count, name);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Blob enumeration cancelled after {Count} items.", count);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            _logger.LogInformation("The container '{ContainerName}' contains {Count} blobs.", name, count);
            return (count, false, null);
        }

        // Otherwise use SDK AsPages to support continuation tokens and limits.
        var connection = _settings.ConnectionString;
        var client = new BlobServiceClient(connection);
        var containerClient = client.GetBlobContainerClient(name);

        if (!await containerClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            _logger.LogWarning("The container '{ContainerName}' does not exist.", name);
            return (0L, false, null);
        }

        long total = 0;
        string? nextContinuation = continuationToken;
        bool truncated = false;

        await foreach (var page in containerClient.GetBlobsAsync().AsPages(continuationToken, pageSize).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            foreach (BlobItem item in page.Values)
            {
                total++;
                if ((total % 10000) == 0)
                {
                    _logger.LogInformation("Enumerated {Count} blobs so far in container '{ContainerName}'.", total, name);
                }

                if (total >= maxItems)
                {
                    // record continuation token for next run and stop
                    nextContinuation = page.ContinuationToken;
                    truncated = true;
                    break;
                }
            }

            if (truncated)
            {
                break;
            }

            nextContinuation = page.ContinuationToken;
            // if no continuation token, we've reached the end
            if (string.IsNullOrEmpty(nextContinuation))
            {
                truncated = false;
                break;
            }
        }

        _logger.LogInformation("CountBlobsAsync for container '{ContainerName}' finished: Count={Count}, IsTruncated={IsTruncated}", name, total, truncated);
        return (total, truncated, truncated ? nextContinuation : null);
    }
}
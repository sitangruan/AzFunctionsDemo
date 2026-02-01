using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlobTimerFunc.Services;
public sealed class BlobListerPaged : IBlobLister
{
    private readonly BlobServiceClient _client;
    private readonly BlobSettings _settings;
    private readonly ILogger<BlobListerPaged> _logger;

    public BlobListerPaged(IOptions<BlobSettings> settings, ILogger<BlobListerPaged> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = new BlobServiceClient(_settings.ConnectionString);
    }

    /// <summary>
    /// Counts up to maxItems blobs using paged enumeration.
    /// Returns (count, isTruncated, continuationToken).
    /// If isTruncated==true, pass continuationToken to resume next run.
    /// </summary>
    public async Task<(long Count, bool IsTruncated, string? ContinuationToken)> CountBlobsAsync(
        string? containerName = null,
        long maxItems = long.MaxValue,
        int pageSize = 500,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        if (maxItems <= 0) throw new ArgumentOutOfRangeException(nameof(maxItems));
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var name = containerName ?? _settings.ContainerName;
        var container = _client.GetBlobContainerClient(name);

        if (!await container.ExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            _logger.LogWarning("Container '{ContainerName}' does not exist.", name);
            return (0, false, null);
        }

        long count = 0;
        string? nextContinuation = continuationToken;
        bool truncated = false;

        await foreach (var page in container.GetBlobsAsync().AsPages(continuationToken, pageSize).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            // Iterate items in the page
            foreach (BlobItem item in page.Values)
            {
                count++;
                if ((count % 10000) == 0)
                {
                    _logger.LogInformation("Enumerated {Count} blobs so far in container '{ContainerName}'.", count, name);
                }

                if (count >= maxItems)
                {
                    // Reached the configured limit — record continuation token for next run.
                    nextContinuation = page.ContinuationToken;
                    truncated = true;
                    break;
                }
            }

            if (truncated)
            {
                break;
            }

            // If there is a continuation token and we haven't reached the limit, store it to allow resumption.
            nextContinuation = page.ContinuationToken;

            // No continuation token => reached end
            if (string.IsNullOrEmpty(nextContinuation))
            {
                truncated = false;
                break;
            }
        }

        _logger.LogInformation("CountBlobsWithLimitAsync for container '{ContainerName}' finished: Count={Count}, IsTruncated={IsTruncated}", name, count, truncated);
        return (count, truncated, truncated ? nextContinuation : null);
    }
}
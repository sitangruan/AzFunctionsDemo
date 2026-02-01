using BlobTimerFunc.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlobTimerFunc;

public class CheckImages
{
    private readonly IBlobLister _blobLister;
    private readonly ILogger<CheckImages> _logger;
    private readonly string _containerName;

    public CheckImages(IBlobLister blobLister, ILogger<CheckImages> logger, IConfiguration config)
    {
        _blobLister = blobLister;
        _logger = logger;
        _containerName = config["BlobContainerName"] ?? "images";
    }

    [Function("CheckBlobTimer")]
    public async Task Run([TimerTrigger("%TimerSchedule%")] TimerInfo myTimer, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Check start time (UTC): {StartTime}", DateTimeOffset.UtcNow);

        try
        {
            // Example: no limit, read full count
            var (count, isTruncated, continuationToken) = await _blobLister.CountBlobsAsync(_containerName, cancellationToken: cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Found {Count} blobs in container '{ContainerName}'. Truncated={IsTruncated}", count, _containerName, isTruncated);

            // If you want to use paging/limits, you can pass maxItems and continuationToken saved from prior runs:
            // var (countPart, truncated, nextToken) = await _blobLister.CountBlobsAsync(_containerName, maxItems:100000, pageSize:500, continuationToken: savedToken, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Function cancelled during blob count.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting blobs in '{ContainerName}'.", _containerName);
            throw;
        }
    }
}
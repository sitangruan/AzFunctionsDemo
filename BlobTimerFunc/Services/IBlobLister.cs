namespace BlobTimerFunc.Services;
public interface IBlobLister
{
    /// <summary>
    /// Count blobs with optional limits and resume token support.
    /// Returns (Count, IsTruncated, ContinuationToken).
    /// - If IsTruncated == true, pass ContinuationToken to resume next run.
    /// </summary>
    Task<(long Count, bool IsTruncated, string? ContinuationToken)> CountBlobsAsync(
        string? containerName = null,
        long maxItems = long.MaxValue,
        int pageSize = 500,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);
}
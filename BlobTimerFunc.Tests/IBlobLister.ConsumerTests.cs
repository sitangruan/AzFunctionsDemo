using BlobTimerFunc.Services;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BlobTimerFunc.Tests;

public class IBlobLister_ConsumerTests
{
    // A tiny consumer used only for test demonstration.
    private sealed class BlobCountReporter
    {
        private readonly IBlobLister _lister;
        public BlobCountReporter(IBlobLister lister) => _lister = lister;
        public async Task<long> GetCountAsync(CancellationToken ct = default)
        {
            var (count, _, _) = await _lister.CountBlobsAsync(null, cancellationToken: ct).ConfigureAwait(false);
            return count;
        }
    }

    [Fact]
    public async Task Consumer_Uses_IBlobLister_CountBlobsAsync()
    {
        // Arrange
        var mockLister = new Mock<IBlobLister>(MockBehavior.Strict);
        mockLister
            .Setup(x => x.CountBlobsAsync(null, It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((42L, false, null))
            .Verifiable();

        var reporter = new BlobCountReporter(mockLister.Object);

        // Act
        var value = await reporter.GetCountAsync(CancellationToken.None);

        // Assert
        Assert.Equal(42L, value);
        mockLister.Verify();
    }

    [Fact]
    public async Task Consumer_Propagates_CancellationToken_To_IBlobLister()
    {
        // Arrange
        var mockLister = new Mock<IBlobLister>();
        mockLister
            .Setup(x => x.CountBlobsAsync(null, It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((0L, false, null));

        var reporter = new BlobCountReporter(mockLister.Object);
        using var cts = new CancellationTokenSource();

        // Act
        await reporter.GetCountAsync(cts.Token);

        // Assert: verify the invocation occurred
        mockLister.Verify(x => x.CountBlobsAsync(null, It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
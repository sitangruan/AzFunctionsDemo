using BlobTimerFunc.Services;
using FuncUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BlobTimerFunc.Tests;

public class BlobLister_UnitTests
{
    private static async IAsyncEnumerable<string> ToAsyncEnumerable(params string[] items)
    {
        foreach (var s in items)
        {
            yield return s;
            await Task.Yield();
        }
    }

    [Fact]
    public async Task CountBlobsAsync_Returns_CorrectCount_WhenContainerExists()
    {
        var mockAdapter = new Mock<IBlobStorageAdapter>(MockBehavior.Strict);
        mockAdapter.Setup(a => a.ContainerExistsAsync("images", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);
        mockAdapter.Setup(a => a.ListBlobNamesAsync("images", It.IsAny<CancellationToken>()))
                   .Returns(() => ToAsyncEnumerable("a", "b", "c"));

        var settings = Options.Create(new BlobSettings { ConnectionString = "UseDevelopmentStorage=true", ContainerName = "images" });
        var logger = NullLogger<BlobLister>.Instance;

        var lister = new BlobLister(settings, mockAdapter.Object, logger);

        var (count, isTruncated, token) = await lister.CountBlobsAsync(null, cancellationToken: CancellationToken.None);

        Assert.Equal(3, count);
        Assert.False(isTruncated);
        Assert.Null(token);
        mockAdapter.VerifyAll();
    }

    [Fact]
    public async Task CountBlobsAsync_Returns_Zero_WhenContainerDoesNotExist()
    {
        var mockAdapter = new Mock<IBlobStorageAdapter>(MockBehavior.Strict);
        mockAdapter.Setup(a => a.ContainerExistsAsync("images", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

        var settings = Options.Create(new BlobSettings { ConnectionString = "UseDevelopmentStorage=true", ContainerName = "images" });
        var logger = NullLogger<BlobLister>.Instance;

        var lister = new BlobLister(settings, mockAdapter.Object, logger);

        var (count, isTruncated, token) = await lister.CountBlobsAsync(null, cancellationToken: CancellationToken.None);

        Assert.Equal(0, count);
        Assert.False(isTruncated);
        Assert.Null(token);
        mockAdapter.Verify(a => a.ListBlobNamesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
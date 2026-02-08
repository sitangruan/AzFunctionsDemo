using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using BlobTimerFunc.Services;

namespace BlobTimerFunc.Tests;


[Collection("Azure collection")]
public class BlobLister_IntegrationTests
{
    private readonly AzureFixture _fixture;

    public BlobLister_IntegrationTests(AzureFixture fixture)
    {
        _fixture = fixture;
    }

    // Ensure Azurite is running or point this at a real account connection string.
    [Fact]
    public async Task CountBlobsAsync_Returns_NumberOfBlobs_WhenContainerExists()
    {
        if (!_fixture.IsAvailable)
        {
            // Azure storage not available in this environment — skip test quickly.
            return;
        }

        var containerName = "itest" + Guid.NewGuid().ToString("n").Substring(0, 8);
        var serviceClient = _fixture.Client;
        var container = serviceClient.GetBlobContainerClient(containerName);

        try
        {
            await container.CreateIfNotExistsAsync();
            // upload 3 blobs
            await container.UploadBlobAsync("a.txt", BinaryData.FromString("one"));
            await container.UploadBlobAsync("b.txt", BinaryData.FromString("two"));
            await container.UploadBlobAsync("c.txt", BinaryData.FromString("three"));

            var settings = Options.Create(new BlobSettings { ConnectionString = _fixture.ConnectionString, ContainerName = containerName });
            var adapter = new BlobStorageAdapter(settings, NullLogger<BlobStorageAdapter>.Instance);
            var lister = new BlobLister(settings, adapter, NullLogger<BlobLister>.Instance);

            var (count, isTruncated, token) = await lister.CountBlobsAsync(containerName, cancellationToken: CancellationToken.None);

            Assert.Equal(3, count);
            Assert.False(isTruncated);
            Assert.Null(token);
        }
        finally
        {
            await container.DeleteIfExistsAsync();
        }
    }

    [Fact]
    public async Task CountBlobsAsync_WithLimit_Returns_TruncatedToken_And_Can_Resume()
    {
        if (!_fixture.IsAvailable)
        {
            return;
        }

        var containerName = "itest" + Guid.NewGuid().ToString("n").Substring(0, 8);
        var serviceClient = _fixture.Client;
        var container = serviceClient.GetBlobContainerClient(containerName);

        try
        {
            await container.CreateIfNotExistsAsync();
            // upload 5 blobs
            for (int i = 0; i < 5; i++)
            {
                await container.UploadBlobAsync($"blob{i}.txt", BinaryData.FromString($"payload {i}"));
            }

            var settings = Options.Create(new BlobSettings { ConnectionString = _fixture.ConnectionString, ContainerName = containerName });
            var adapter = new BlobStorageAdapter(settings, NullLogger<BlobStorageAdapter>.Instance);
            var lister = new BlobLister(settings, adapter, NullLogger<BlobLister>.Instance);

            // First run: limit to 2 items
            var (count1, truncated1, token1) = await lister.CountBlobsAsync(containerName, maxItems: 2, pageSize: 2, continuationToken: null, cancellationToken: CancellationToken.None);

            Assert.Equal(2, count1);
            Assert.True(truncated1);
            Assert.False(string.IsNullOrWhiteSpace(token1));

            // Resume from token to count remaining items
            var (count2, truncated2, token2) = await lister.CountBlobsAsync(containerName, maxItems: long.MaxValue, pageSize: 2, continuationToken: token1, cancellationToken: CancellationToken.None);

            // count2 should be remaining items (5 - 2 = 3)
            Assert.Equal(3, count2);
            Assert.False(truncated2);
            Assert.Null(token2);
        }
        finally
        {
            await container.DeleteIfExistsAsync();
        }
    }
}
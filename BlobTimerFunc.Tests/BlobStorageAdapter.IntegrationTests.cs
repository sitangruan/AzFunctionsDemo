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
public class BlobStorageAdapter_IntegrationTests
{
    private readonly AzureFixture _fixture;

    public BlobStorageAdapter_IntegrationTests(AzureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task BlobStorageAdapter_List_and_Exists_Work_With_Azurite()
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
            await container.UploadBlobAsync("one.txt", BinaryData.FromString("1"));
            await container.UploadBlobAsync("two.txt", BinaryData.FromString("2"));

            var settings = Options.Create(new BlobSettings { ConnectionString = _fixture.ConnectionString, ContainerName = containerName });
            var logger = NullLogger<BlobStorageAdapter>.Instance;
            var adapter = new BlobStorageAdapter(settings, logger);

            Assert.True(await adapter.ContainerExistsAsync(containerName, CancellationToken.None));

            int items = 0;
            await foreach (var name in adapter.ListBlobNamesAsync(containerName, CancellationToken.None))
            {
                Assert.False(string.IsNullOrWhiteSpace(name));
                items++;
            }

            Assert.Equal(2, items);
        }
        finally
        {
            await container.DeleteIfExistsAsync();
        }
    }
}
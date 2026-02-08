using System;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Xunit;

namespace BlobTimerFunc.Tests;

/// <summary>
/// Shared fixture for integration tests that need Blob storage (Azurite or real account).
/// It chooses a connection string from environment/app-settings in this order:
/// 1) BlobConnectionString
/// 2) AzureWebJobsStorage
/// 3) AZURE_STORAGE_CONNECTION_STRING
/// 4) DefaultLocalConnection ("UseDevelopmentStorage=true")
/// </summary>
public class AzureFixture : IAsyncLifetime
{
    public const string DefaultLocalConnection = "UseDevelopmentStorage=true";

    public string ConnectionString { get; private set; }
    public BlobServiceClient Client { get; private set; }
    /// <summary>True when the storage endpoint was reachable during InitializeAsync.</summary>
    public bool IsAvailable { get; private set; }

    public AzureFixture()
    {
        // Prefer an explicit Blob connection string if provided (e.g. in local.settings.json Values or Azure App Settings)
        ConnectionString =
            Environment.GetEnvironmentVariable("BlobConnectionString")
            ?? Environment.GetEnvironmentVariable("AzureWebJobsStorage")
            ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")
            ?? DefaultLocalConnection;

        Client = new BlobServiceClient(ConnectionString);
        IsAvailable = false;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Probe the service to make sure it's available (simple read operation).
            await Client.GetPropertiesAsync().ConfigureAwait(false);
            IsAvailable = true;
        }
        catch (RequestFailedException)
        {
            IsAvailable = false;
        }
        catch (Exception)
        {
            IsAvailable = false;
        }
    }

    public Task DisposeAsync()
    {
        // No special disposal required for BlobServiceClient.
        return Task.CompletedTask;
    }
}
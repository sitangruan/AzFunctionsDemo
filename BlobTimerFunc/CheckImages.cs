using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace BlobTimerFunc;

public class CheckImages
{
    private readonly ILogger _logger;
    private readonly string _connectionString;
    private readonly string _containerName;

    public CheckImages(ILoggerFactory loggerFactory, IConfiguration config)
    {
        _logger = loggerFactory.CreateLogger<CheckImages>();
        _connectionString = config["BlobConnectionString"] ?? "UseDevelopmentStorage=true";
        _containerName = config["BlobContainerName"] ?? "images";
    }

    [Function("CheckBlobTimer")]
    public async Task Run([TimerTrigger("%TimerSchedule%")] TimerInfo myTimer)
    {
        _logger.LogInformation($"Check start time: {DateTime.Now}");

        try
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Check if the container exists
            if (await containerClient.ExistsAsync())
            {
                var blobs = containerClient.GetBlobsAsync();
                int count = 0;
                await foreach (var blob in blobs)
                {
                    count++;
                }

                _logger.LogInformation($"Hello! The container \"{_containerName}\" {count} files.");
            }
            else
            {
                _logger.LogWarning("The container does not exist.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
        }
    }
}
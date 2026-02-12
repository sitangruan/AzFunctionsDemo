using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DurableFunc;

public class BlobUploadedDurable
{
    private readonly ILogger<BlobUploadedDurable> _logger;

    public BlobUploadedDurable(ILogger<BlobUploadedDurable> logger)
    {
        _logger = logger;
    }

    [Function(nameof(BlobUploadedDurable))]
    public async Task Run([BlobTrigger("%BlobContainerName%/{name}", Connection = "BlobConnectionString")] Stream stream, string name, [DurableClient] DurableTaskClient client)
    {
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(ProcessBlobOrchestrator), name);
    }
}
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
        //string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(ProcessBlobOrchestrator), name); // This is the original line that starts the orchestration with a single instance. We will replace it with the fan-out/fan-in version below.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(ProcessBlobOrchestratorFanOutFanIn), name); // This line starts the orchestration with the fan-out/fan-in version, which allows for processing multiple blobs in parallel if needed.
    }
}
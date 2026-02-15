using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DurableFunc;

public static class ProcessBlobOrchestratorFanOutFanIn
{
    [Function(nameof(ProcessBlobOrchestratorFanOutFanIn))]
    public static async Task<FanOutFanInProcessingResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var fileNames = new List<string> { "file1.txt", "file2.txt", "file3.txt" };
        var log = context.CreateReplaySafeLogger(nameof(ProcessBlobOrchestrator));

        var parallelTasks = new List<Task<string>>();

        log.LogInformation("Start to distribute the tasks (Fan-out)...");

        foreach (var name in fileNames)
        {
            Task<string> task = context.CallActivityAsync<string>(nameof(ActivityFuncs.AnalyzeFileActivity), name);
            parallelTasks.Add(task);
        }

        // 2. [Wait for completion] (Fan-in)
        log.LogInformation("All tasks are distributed. Waiting for completion...");
        string[] allResults = await Task.WhenAll(parallelTasks);

        // 3. Send email for combined results
        var summary = string.Join(", ", allResults);
        await context.CallActivityAsync(nameof(ActivityFuncs.SendEmailActivity), summary);

        return new FanOutFanInProcessingResult { TotalProcessed = allResults.Length, Details = allResults };
    }
}

public class FanOutFanInProcessingResult
{
    public int TotalProcessed { get; set; }
    public string[] Details { get; set; } = [];
}
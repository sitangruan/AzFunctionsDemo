using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DurableFunc;

public class ProcessBlobOrchestratorFanOutFanIn
{
    [Function(nameof(ProcessBlobOrchestratorFanOutFanIn))]
    public async Task<FanOutFanInProcessingResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var fileNames = new List<string> { "file1.txt", "file2.txt", "file3.txt" };
        var log = context.CreateReplaySafeLogger(nameof(ProcessBlobOrchestrator));

        var parallelTasks = new List<Task<string>>();

        log.LogInformation("Start to distribute the tasks (Fan-out)...");

        foreach (var name in fileNames)
        {
            //Task<string> task = context.CallActivityAsync<string>(nameof(ActivityFuncs.AnalyzeFileActivity), name);
            //parallelTasks.Add(task); // This is the original code without exception handling
            parallelTasks.Add(CaptureExceptionAsync(context, name)); // This is the modified code with exception handling
        }

        // 2. [Wait for completion] (Fan-in)
        log.LogInformation("All tasks are distributed. Waiting for completion...");
        string[] allResults = await Task.WhenAll(parallelTasks);

        var successCount = allResults.Count(r => !r.StartsWith("Error:"));
        var failCount = allResults.Length - successCount;
        var resultCountMessage = $"Total tasks: {allResults.Length}, Success: {successCount}, Failed: {failCount}";

        log.LogInformation(resultCountMessage);

        // 3. Send email for combined results
        var summary = string.Join(", ", allResults);
        await context.CallActivityAsync(nameof(ActivityFuncs.SendEmailActivity), summary + " - " + resultCountMessage);

        return new FanOutFanInProcessingResult { TotalProcessed = allResults.Length, Details = allResults };
    }

    private async Task<string> CaptureExceptionAsync(TaskOrchestrationContext context, string name)
    {
        try
        {
            return await context.CallActivityAsync<string>(nameof(ActivityFuncs.AnalyzeFileActivity), name);
        }
        catch (Exception ex)
        {
            return $"Error: {name} failed - {ex.Message}";
        }
    }
}

public class FanOutFanInProcessingResult
{
    public int TotalProcessed { get; set; }
    public string[] Details { get; set; } = [];
}
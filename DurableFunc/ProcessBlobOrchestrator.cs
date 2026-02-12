using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DurableFunc;

public static class ProcessBlobOrchestrator
{
    [Function(nameof(ProcessBlobOrchestrator))]
    public static async Task<object> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        // 1. Get the file name from the input
        string fileName = context.GetInput<string>() ?? string.Empty;
        var log = context.CreateReplaySafeLogger(nameof(ProcessBlobOrchestrator));

        // --- Step 1: analyze the file ---
        log.LogInformation("Analyzing the file...");
        var fileData = await context.CallActivityAsync<string>(nameof(ActivityFuncs.AnalyzeFileActivity), fileName);

        // --- Step 2: send email ---
        log.LogInformation("Sending the email...");
        var emailResult = await context.CallActivityAsync<string>(nameof(ActivityFuncs.SendEmailActivity), fileName);

        // --- Step 3: update databse ---
        log.LogInformation("Updating the DB...");
        var dbResult = await context.CallActivityAsync<bool>(nameof(ActivityFuncs.UpdateDatabaseActivity), fileData);

        return new
        {
            FileName = fileName,
            Analysis = fileData,
            EmailStatus = emailResult,
            DatabaseStatus = dbResult ? "Success" : "Failed"
        };
    }
}
using Microsoft.Azure.Functions.Worker;

namespace DurableFunc;

public static class ActivityFuncs
{
    // Activity 1: Anayze File
    [Function(nameof(AnalyzeFileActivity))]
    public static string AnalyzeFileActivity([ActivityTrigger] string name)
    {
        return $"[File Report] {name} has 100 rows content.";
    }

    // Activity 2: Send email
    [Function(nameof(SendEmailActivity))]
    public static string SendEmailActivity([ActivityTrigger] string name)
    {
        // To do: use Mailkit to send email
        return $"Email has been sent";
    }

    // Activity 3: Mimic database update
    [Function(nameof(UpdateDatabaseActivity))]
    public static bool UpdateDatabaseActivity([ActivityTrigger] string report)
    {
        // Mimic database update, return true if success, false if failed
        return true;
    }
}
using FuncUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DurableFunc;

public class ActivityFuncs
{
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;
    private readonly ILogger<ActivityFuncs> _logger;

    public ActivityFuncs(IEmailSender emailSender, IConfiguration config, ILogger<ActivityFuncs> logger)
    {
        _emailSender = emailSender;
        _config = config;
        _logger = logger;
    }

    // Activity 1: Anayze File
    [Function(nameof(AnalyzeFileActivity))]
    public string AnalyzeFileActivity([ActivityTrigger] string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(name));
        }

        if (name.Contains("file2", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Simulated failure for file: {name}");
        }

        _logger.LogInformation("AnalyzeFileActivity {name}...", name);
        return $"[File Report] {name} has 100 rows content.";
    }

    // Activity 2: Send email
    [Function(nameof(SendEmailActivity))]
    public async Task<string> SendEmailActivity([ActivityTrigger] string name)
    {
        _logger.LogInformation("SendEmailActivity {name}...", name);
        // Build email content
        var subject = $"New blob uploaded: {name}";
        var htmlBody = $"A new blob named <strong>{name}</strong> was uploaded.";

        // Read recipient from configuration; fallback to a default placeholder.
        var recipient = _config["NotificationRecipient"] ?? _config["SmtpTo"] ?? "recipient@example.test";

        // Send email (implementation provided by registered IEmailSender)
        await _emailSender.SendAsync(recipient, subject, htmlBody, CancellationToken.None).ConfigureAwait(false);
        return $"Email has been sent";
    }

    // Activity 3: Mimic database update
    [Function(nameof(UpdateDatabaseActivity))]
    public bool UpdateDatabaseActivity([ActivityTrigger] string report)
    {
        _logger.LogInformation("UpdateDatabaseActivity {report}...", report);
        // Mimic database update, return true if success, false if failed
        return true;
    }
}
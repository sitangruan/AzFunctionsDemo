using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlobTriggerFunc.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlobTriggerFunc;

public class BlobUploaded
{
    private readonly ILogger<BlobUploaded> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public BlobUploaded(IEmailSender emailSender, IConfiguration config, ILogger<BlobUploaded> logger)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Blob trigger: container token and connection key are read from configuration.
    [Function(nameof(BlobUploaded))]
    public async Task Run(
        [BlobTrigger("%BlobContainerName%/{name}", Connection = "BlobConnectionString")] Stream blobStream,
        string name,
        CancellationToken cancellationToken)
    {
        try
        {
            // Try to get length without fully buffering the stream when possible.
            long length = 0;
            if (blobStream.CanSeek)
            {
                length = blobStream.Length;
            }
            else
            {
                // If not seekable, read but avoid keeping large buffer in memory for production.
                // Here we copy to a temporary MemoryStream for size reporting only; adjust for large blobs.
                using var ms = new MemoryStream();
                await blobStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                length = ms.Length;
            }

            _logger.LogInformation("Blob trigger fired for blob: {BlobName}, size={Size} bytes", name, length);

            // Build email content
            var subject = $"New blob uploaded: {name}";
            var htmlBody = $"A new blob named <strong>{name}</strong> was uploaded. Size: {length} bytes.";

            // Read recipient from configuration; fallback to a default placeholder.
            var recipient = _config["NotificationRecipient"] ?? _config["SmtpTo"] ?? "recipient@example.test";

            // Send email (implementation provided by registered IEmailSender)
            await _emailSender.SendAsync(recipient, subject, htmlBody, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Notification email sent to {Recipient} for blob {BlobName}", recipient, name);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Blob processing was cancelled for {BlobName}", name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing blob {BlobName}", name);
            // Do not swallow exceptions so Functions host can handle retries/logging as configured.
            throw;
        }
    }
}
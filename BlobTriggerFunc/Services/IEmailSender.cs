namespace BlobTriggerFunc.Services;

/// <summary>
/// Abstraction for sending email messages.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Send an HTML email.
    /// </summary>
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BlobTriggerFunc.Services;

/// <summary>
/// MailKit-based email sender that reads SMTP settings from IConfiguration.
/// </summary>
public class MailKitEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IOptions<SmtpOptions> options, ILogger<MailKitEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var secureSocket = _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(_options.Host, _options.Port, secureSocket, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(_options.User) && !string.IsNullOrEmpty(_options.Pass))
            {
                await client.AuthenticateAsync(_options.User, _options.Pass, cancellationToken).ConfigureAwait(false);
            }

            var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Email sent to {To} with subject '{Subject}'", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
        finally
        {
            try
            {
                await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // ignore disconnect errors
            }
        }
    }
}
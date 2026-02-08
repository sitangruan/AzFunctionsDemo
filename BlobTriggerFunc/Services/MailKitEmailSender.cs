using MailKit.Security;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlobTriggerFunc.Services;

/// <summary>
/// MailKit-based email sender that reads SMTP settings from IConfiguration.
/// </summary>
public class MailKitEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string? _user;
    private readonly string? _pass;
    private readonly string _from;
    private readonly bool _useSsl;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IConfiguration config, ILogger<MailKitEmailSender> logger)
    {
        _logger = logger;
        _host = config["SmtpHost"] ?? throw new InvalidOperationException("SmtpHost not configured");
        _port = int.TryParse(config["SmtpPort"], out var p) ? p : 587;
        _user = config["SmtpUser"];
        _pass = config["SmtpPass"];
        _from = config["SmtpFrom"] ?? "noreply@example.test";
        _useSsl = bool.TryParse(config["SmtpUseSsl"], out var s) ? s : true;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_from));
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
            var secureSocket = _useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(_host, _port, secureSocket, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(_user) && !string.IsNullOrEmpty(_pass))
            {
                await client.AuthenticateAsync(_user, _pass, cancellationToken).ConfigureAwait(false);
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
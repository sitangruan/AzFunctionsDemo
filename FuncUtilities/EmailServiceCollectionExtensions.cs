using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FuncUtilities;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddMailKitEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(opts =>
        {
            opts.Host = configuration["SmtpHost"] ?? opts.Host;
            opts.Port = int.TryParse(configuration["SmtpPort"], out var p) ? p : opts.Port;
            opts.User = configuration["SmtpUser"] ?? opts.User;
            opts.Pass = configuration["SmtpPass"] ?? opts.Pass;
            opts.From = configuration["SmtpFrom"] ?? opts.From;
            opts.UseSsl = bool.TryParse(configuration["SmtpUseSsl"], out var s) ? s : opts.UseSsl;
        });

        services.AddTransient<IEmailSender, MailKitEmailSender>();

        return services;
    }
}
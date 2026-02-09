using BlobTriggerFunc;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlobTriggerFunc.Services;

var builder = FunctionsApplication.CreateBuilder(args);

// Configure Functions worker host
builder.ConfigureFunctionsWebApplication();

// Read configuration (local.settings.json or Azure Application Settings)
var config = builder.Configuration;

// Bind BlobSettings from configuration sources so you can use "%BlobContainerName%" in attributes.
builder.Services.Configure<BlobSettings>(opts =>
{
    opts.ConnectionString =
        config["BlobConnectionString"]
        ?? config.GetValue<string>("BlobSettings:ConnectionString")
        ?? opts.ConnectionString;

    opts.ContainerName =
        config["BlobContainerName"]
        ?? config.GetValue<string>("BlobSettings:ContainerName")
        ?? opts.ContainerName;
});

builder.Services.Configure<SmtpOptions>(opts =>
{
    opts.Host = config["SmtpHost"] ?? opts.Host;
    opts.Port = int.TryParse(config["SmtpPort"], out var p) ? p : opts.Port;
    opts.User = config["SmtpUser"] ?? opts.User;
    opts.Pass = config["SmtpPass"] ?? opts.Pass;
    opts.From = config["SmtpFrom"] ?? opts.From;
    opts.UseSsl = bool.TryParse(config["SmtpUseSsl"], out var s) ? s : opts.UseSsl;
});

// Register your email sender implementation
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();

builder.Build().Run();

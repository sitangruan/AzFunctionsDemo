using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BlobTimerFunc;
using BlobTimerFunc.Services;
using FuncUtilities;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

var config = builder.Configuration;

// Try to obtain connection string in this order:
// 1) configuration (local.settings.json / App Settings) BlobConnectionString
// 2) configuration BlobSettings:ConnectionString
// 3) if KeyVaultUri present, read secret "BlobConnectionString" from Key Vault using DefaultAzureCredential
// 4) fallback to existing default in BlobSettings
var kvUri = config["KeyVaultUri"];
string? connectionString = config["BlobConnectionString"] ?? config.GetValue<string>("BlobSettings:ConnectionString");

if (string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(kvUri))
{
    try
    {
        var secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
        var secret = secretClient.GetSecret("BlobConnectionString");
        connectionString = secret.Value?.Value;
    }
    catch (Exception)
    {
        // don't fail startup — fall back to other configuration sources
        connectionString = null;
    }
}

// Bind BlobSettings using the resolved connectionString (if any)
builder.Services.Configure<BlobSettings>(opts =>
{
    opts.ConnectionString = connectionString
        ?? config["BlobConnectionString"]
        ?? config.GetValue<string>("BlobSettings:ConnectionString")
        ?? opts.ConnectionString;

    opts.ContainerName =
        config["BlobContainerName"]
        ?? config.GetValue<string>("BlobSettings:ContainerName")
        ?? opts.ContainerName;
});

// Register adapter + lister
builder.Services.AddSingleton<IBlobStorageAdapter, BlobStorageAdapter>();
builder.Services.AddSingleton<IBlobLister, BlobLister>();

// Register Application Insights worker telemetry (keep this; it exists in Worker SDK)
builder.Services.AddApplicationInsightsTelemetryWorkerService();

builder.Build().Run();
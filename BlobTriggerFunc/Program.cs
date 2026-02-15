using FuncUtilities;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

builder.Services.AddMailKitEmailSender(configuration: config);

builder.Build().Run();

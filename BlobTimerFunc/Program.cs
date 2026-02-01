using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using BlobTimerFunc.Services;
using BlobTimerFunc;
using Microsoft.Azure.Functions.Worker;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

var config = builder.Configuration;

// Bind BlobSettings
builder.Services.Configure<BlobSettings>(opts =>
{
    opts.ConnectionString = config["BlobConnectionString"] ?? config.GetValue<string>("BlobSettings:ConnectionString") ?? opts.ConnectionString;
    opts.ContainerName = config["BlobContainerName"] ?? config.GetValue<string>("BlobSettings:ContainerName") ?? opts.ContainerName;
});

// Register adapter + lister
builder.Services.AddSingleton<IBlobStorageAdapter, BlobStorageAdapter>();
builder.Services.AddSingleton<IBlobLister, BlobLister>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
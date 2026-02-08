namespace BlobTriggerFunc;

public sealed class BlobSettings
{
    // Connection string for Blob storage, e.g. "UseDevelopmentStorage=true" for Azurite local testing.
    public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";

    // Container name used by the BlobTrigger and other operations.
    public string ContainerName { get; set; } = "images2";
}
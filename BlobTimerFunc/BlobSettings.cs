namespace BlobTimerFunc;
public sealed class BlobSettings
{
    public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";
    public string ContainerName { get; set; } = "images";
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlobTimerFunc.Services;
public interface IBlobStorageAdapter
{
    Task<bool> ContainerExistsAsync(string containerName, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> ListBlobNamesAsync(string containerName, CancellationToken cancellationToken = default);
}
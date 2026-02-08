using Xunit;

namespace BlobTimerFunc.Tests;

// Define a collection that shares AzureFixture across multiple test classes.
// DisableParallelization ensures tests that share the fixture do not run concurrently.
[CollectionDefinition("Azure collection", DisableParallelization = true)]
public class AzureCollection : ICollectionFixture<AzureFixture>
{
    // Intentionally empty; this class just defines the collection and associates the fixture.
}
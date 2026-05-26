using Xunit;

namespace IndustrialPress.RestApi.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Integration";
}

using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Infrastructure.Persistence;
using Shared.Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Shared.Positions.Tests.Integration;

public sealed class SharedPositionsApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<SharedPositionsApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly SharedPositionsApiFactory Factory;

    protected BaseIntegrationTest(SharedPositionsApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<SharedDbContext>(SharedSeed.All);
    }
}

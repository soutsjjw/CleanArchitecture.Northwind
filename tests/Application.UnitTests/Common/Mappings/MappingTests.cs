using CleanArchitecture.Northwind.Application.Common.Mappings;
using Mapster;
using NUnit.Framework;

namespace CleanArchitecture.Northwind.Application.UnitTests.Common.Mappings;

public class MappingTests
{
    private TypeAdapterConfig? _config;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _config = new TypeAdapterConfig();
        MapsterConfiguration.RegisterMappings(_config);
        _config.Compile();
    }

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        // Verify that the configuration compiles without errors
        Assert.DoesNotThrow(() => _config!.Compile());
    }
}

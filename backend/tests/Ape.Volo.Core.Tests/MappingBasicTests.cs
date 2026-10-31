using Ape.Volo.Core.Mapping;
using Mapster;
using Xunit;

namespace Ape.Volo.Core.Tests;

public class MappingBasicTests
{
    [Fact]
    public void BothMappingOverloadsUseRegisteredConfiguration()
    {
        var configuration = new TypeAdapterConfig();
        configuration.NewConfig<Source, Destination>()
            .Map(destination => destination.DisplayName, source => "Mapped: " + source.Name);
        var mapper = new Mapper(configuration);
        var source = new Source { Name = "ApeVolo" };

        Assert.Equal("Mapped: ApeVolo", mapper.MapTo<Source, Destination>(source).DisplayName);
        Assert.Equal("Mapped: ApeVolo", mapper.MapTo<Destination>(source).DisplayName);
    }

    public class Source
    {
        public string Name { get; set; } = string.Empty;
    }

    public class Destination
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}

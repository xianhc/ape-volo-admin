using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Ape.Volo.Common.Tests;

public class SerializationBasicTests
{
    [Fact]
    public void LongIdsSerializeWithoutPrecisionLoss()
    {
        var json = JObject.Parse(new { Id = long.MaxValue, Count = 1 }.ToJson());
        Assert.Equal(JTokenType.String, json["id"]!.Type);
        Assert.Equal(long.MaxValue.ToString(), json["id"]!.Value<string>());
        Assert.Equal(JTokenType.Integer, json["count"]!.Type);
    }

    [Fact]
    public void PagedResultsPreserveTotalCountAndItems()
    {
        var json = JObject.Parse(new PagedResultDto<string>(20, ["first", "second"]).ToJson());
        Assert.Equal(20, json["totalCount"]!.Value<int>());
        Assert.Equal(new[] { "first", "second" }, json["items"]!.Values<string>());
    }
}

using Ape.Volo.Application.Contracts.Permission.Auth;
using Ape.Volo.Application.Contracts.Permission.Users.Models;
using Ape.Volo.Common.Converters;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Web.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Ape.Volo.Web.Tests;

public sealed class JsonContractTests
{
    [Fact]
    public void UserSchemaMatchesSerializedJson()
    {
        using var services = Services();
        var generator = services.GetRequiredService<ISchemaGenerator>();
        var schema = Generate<UserVo>(generator);
        var json = JObject.Parse(new UserVo { Id = long.MaxValue, AvatarPath = "/uploads/avatar.png", Password = "must-not-leak" }.ToJson());
        Assert.Equal("string", schema.Properties["id"].Type);
        Assert.Equal(JTokenType.String, json["id"]!.Type);
        Assert.Equal(long.MaxValue.ToString(), json["id"]!.Value<string>());
        Assert.Contains("headerImg", schema.Properties.Keys);
        Assert.NotNull(json["headerImg"]);
        Assert.DoesNotContain("avatarPath", schema.Properties.Keys);
        Assert.DoesNotContain("password", schema.Properties.Keys);
        Assert.Null(json["password"]);
        Assert.Equal("string", schema.Properties["creationTime"].Type);
        Assert.Null(schema.Properties["creationTime"].Format);
    }

    [Fact]
    public void PaginationAndOperationSchemasPreservePrimitiveTypes()
    {
        using var services = Services();
        var generator = services.GetRequiredService<ISchemaGenerator>();
        var page = Generate<PagedResultDto<UserVo>>(generator);
        Assert.Equal("integer", page.Properties["totalCount"].Type);
        var operation = Generate<OperateResult>(generator);
        Assert.Equal("boolean", operation.Properties["isSuccess"].Type);
    }

    [Fact]
    public void TokenSchemaUsesStringTimestamps()
    {
        using var services = Services();
        var schema = Generate<TokenVo>(services.GetRequiredService<ISchemaGenerator>());
        Assert.Equal("string", schema.Properties["expires"].Type);
        Assert.Equal("string", schema.Properties["refreshTokenExpires"].Type);
    }

    private static Microsoft.OpenApi.Models.OpenApiSchema Generate<T>(ISchemaGenerator generator)
    {
        var repository = new SchemaRepository();
        var schema = generator.GenerateSchema(typeof(T), repository);
        return schema.Reference == null ? schema : repository.Schemas[schema.Reference.Id];
    }

    private static ServiceProvider Services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new CustomContractResolver());
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Swagger:Name"] = "api", ["Swagger:Title"] = "Contract", ["Swagger:Version"] = "1"
        }).Build();
        services.AddSwaggerService(configuration);
        return services.BuildServiceProvider();
    }
}

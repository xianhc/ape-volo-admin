using System.ComponentModel.DataAnnotations;
using Ape.Volo.Application.Contracts.Abstractions;
using Ape.Volo.Application.Contracts.Organization.Positions.Models;
using Ape.Volo.Application.Contracts.Identity.Auth;
using Ape.Volo.Application.Contracts.Identity.Users.Models;
using Ape.Volo.Common.Converters;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Entity.Identity.Users;
using Ape.Volo.Web.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Ape.Volo.Web.Tests;

public sealed class JsonContractTests
{
    public static IEnumerable<object[]> CreateUpdateDtoTypes()
        => typeof(CreateUpdatePositionDto).Assembly.GetTypes()
            .Where(type => type.Name.StartsWith("CreateUpdate", StringComparison.Ordinal)
                           && type.Name.EndsWith("Dto", StringComparison.Ordinal))
            .Select(type => new object[] { type });

    [Theory]
    [MemberData(nameof(CreateUpdateDtoTypes))]
    public void CreateUpdateRequestsBindIdsAndExcludeAuditFields(Type dtoType)
    {
        using var services = Services();
        var generator = services.GetRequiredService<ISchemaGenerator>();
        var repository = new SchemaRepository();
        var schema = generator.GenerateSchema(dtoType, repository);
        if (schema.Reference != null) schema = repository.Schemas[schema.Reference.Id];

        const string body = """
            {"id":"9223372036854775807","creatorId":"42","creationTime":"2020-01-01 00:00:00",
             "lastModifierId":"43","lastModificationTime":"2020-02-01 00:00:00","isDeleted":true}
            """;
        var dto = JsonConvert.DeserializeObject(body, dtoType,
            new JsonSerializerSettings { ContractResolver = new CustomContractResolver() });
        Assert.NotNull(dto);
        var json = JObject.Parse(dto.ToJson());
        Assert.Equal("string", schema.Properties["id"].Type);
        Assert.Equal(long.MaxValue.ToString(), json["id"]!.Value<string>());
        foreach (var field in new[] { "creatorId", "creationTime", "lastModifierId", "lastModificationTime", "isDeleted" })
        {
            Assert.DoesNotContain(field, schema.Properties.Keys);
            Assert.Null(json[field]);
        }
    }

    [Fact]
    public void UserRequestsPreserveNestedCollectionsAndValidation()
    {
        using var services = Services();
        var dto = JsonConvert.DeserializeObject<CreateUpdateUserDto>("""
            {"id":"9223372036854775807","userName":"engineer","nickName":"Engineer",
             "email":"engineer@example.com","genderCode":1,"enabled":true,"dept":{"id":"2"},
             "roles":[{"id":"3","name":"Operator"}],"positions":[{"id":"4","name":"Engineer"}]}
            """)!;
        Assert.True(Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true));
        var entity = services.GetRequiredService<IMapper>().MapTo<User>(dto);
        Assert.Equal(long.MaxValue, entity.Id);
        Assert.Equal(dto.UserName, entity.UserName);
        Assert.NotNull(entity.Dept);
        Assert.NotNull(entity.Roles);
        Assert.NotNull(entity.Positions);
        Assert.Equal(2, entity.Dept.Id);
        Assert.Equal(3, Assert.Single(entity.Roles).Id);
        Assert.Equal(4, Assert.Single(entity.Positions).Id);

        var invalid = dto with { Roles = new(), Positions = new(), Email = "invalid" };
        var errors = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(invalid, new ValidationContext(invalid), errors, true));
        Assert.False(Validator.TryValidateProperty(invalid.Roles,
            new ValidationContext(invalid) { MemberName = nameof(dto.Roles) }, null));
        Assert.False(Validator.TryValidateProperty(invalid.Positions,
            new ValidationContext(invalid) { MemberName = nameof(dto.Positions) }, null));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(dto.Email)));
    }

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
        services.AddMappingService();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Swagger:Name"] = "api", ["Swagger:Title"] = "Contract", ["Swagger:Version"] = "1"
        }).Build();
        services.AddSwaggerService(configuration);
        return services.BuildServiceProvider();
    }
}

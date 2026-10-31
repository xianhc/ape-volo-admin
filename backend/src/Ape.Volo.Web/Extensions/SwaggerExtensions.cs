using System.Runtime.InteropServices;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Serilog;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Ape.Volo.Web.Swagger;
using ILogger = Serilog.ILogger;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// swagger扩展配置
/// </summary>
public static class SwaggerExtensions
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(SwaggerExtensions));

    /// <summary>
    /// 注册 Swagger 文档生成服务，并加载各层 XML 注释
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置根对象</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddSwaggerService(this IServiceCollection services, IConfiguration configuration)
    {
        if (services.IsNull()) throw new ArgumentNullException(nameof(services));

        var basePath = AppContext.BaseDirectory;
        var options = configuration.GetConfigurableOptions<SwaggerOptions>();

        services.AddSwaggerGen(c =>
        {
            // 与实际 JSON 转换器一致：long 为字符串，普通日期为不带时区的格式化字符串。
            c.MapType<long>(() => new OpenApiSchema { Type = "string", Pattern = @"^-?\d+$" });
            c.MapType<long?>(() => new OpenApiSchema { Type = "string", Nullable = true, Pattern = @"^-?\d+$" });
            c.MapType<DateTime>(() => new OpenApiSchema { Type = "string", Pattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$" });
            c.MapType<DateTime?>(() => new OpenApiSchema { Type = "string", Nullable = true, Pattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$" });
            c.SchemaFilter<JsonContractSchemaFilter>();
            c.SwaggerDoc(options.Name,
                new OpenApiInfo
                {
                    Version = options.Version,
                    Title = options.Title + "    " + RuntimeInformation.FrameworkDescription,
                    Description = $"Build: {DateTime.Now:yyyy-MM-dd HH:mm:ss}  | User: xianhc",
                    Contact = new OpenApiContact
                    {
                        Name = "xianhc", Email = "apevolo@gamil.com", Url = new Uri("https://www.apevolo.com")
                    }
                });

            try
            {
                var apiXml = Path.Combine(basePath, typeof(SwaggerExtensions).Assembly.GetName().Name + ".xml");
                var applicationXml = Path.Combine(basePath, typeof(Ape.Volo.Application.BaseService<>).Assembly.GetName().Name + ".xml");
                var contractsXml = Path.Combine(basePath, typeof(Ape.Volo.Application.Contracts.Operations.IScheduledJobService).Assembly.GetName().Name + ".xml");
                c.IncludeXmlComments(apiXml, true);
                c.IncludeXmlComments(applicationXml, true);
                c.IncludeXmlComments(contractsXml, true);
                //c.IncludeXmlComments(commonXml, true);
            }
            catch (Exception ex)
            {
                Logger.Error("swagger startup failed\n" + ex.Message);
            }

            // 开启加权小锁
            c.OperationFilter<AddResponseHeadersFilter>();
            c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

            // 在header中添加token，传递到后台
            c.OperationFilter<SecurityRequirementsOperationFilter>();


            // 必须是 oauth2
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token} \"",
                Name = "Authorization", //jwt默认的参数名称
                In = ParameterLocation.Header, //jwt默认存放Authorization信息的位置(请求头中)
                Type = SecuritySchemeType.ApiKey
            });
            //c.SchemaFilter<ExcludeSchemaFilter>();
            //c.CustomSchemaIds(type => type.FullName);
            // c.CustomSchemaIds(type =>
            // {
            //     var typeName = type.Name;
            //
            //     // 处理泛型类型
            //     if (type.IsGenericType)
            //         typeName = type.Name.Split('`')[0] +
            //                    type.GetGenericArguments().Select(a => a.Name).Aggregate((x, y) => $"{x}Of{y}");
            //
            //     // 添加命名空间前缀以避免冲突
            //     return type.Namespace.StartsWith("System") ? $"System{typeName}" :
            //         type.Namespace.Contains("SqlSugar") ? $"SqlSugar{typeName}" : typeName;
            // });
            c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        });
    }

    // public class ExcludeSchemaFilter : ISchemaFilter
    // {
    //     public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    //     {
    //         if (context.Type == typeof(System.Data.DbType))
    //         {
    //             // 将此类型标记为不应生成完整架构
    //             schema.Reference = null;
    //         }
    //     }
    // }
}

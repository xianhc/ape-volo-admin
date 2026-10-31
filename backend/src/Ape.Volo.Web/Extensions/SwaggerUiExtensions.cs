using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Serilog;
using Microsoft.Extensions.Options;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.OpenApi.Models;
using ILogger = Serilog.ILogger;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// Swagger UI 中间件
/// </summary>
public static class SwaggerUiExtensions
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(SwaggerUiExtensions));

    /// <summary>
    /// 启用 Swagger UI，仅当配置 Swagger 开关为 true 时生效
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <param name="streamHtml">返回首页 HTML 流的委托</param>
    /// <exception cref="ArgumentNullException">应用构建器为 null</exception>
    public static void UseSwaggerUiMiddleware(this IApplicationBuilder app, Func<Stream> streamHtml)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));
        var swaggerOptions = app.ApplicationServices.GetRequiredService<IOptions<SwaggerOptions>>().Value;
        if (swaggerOptions.Enabled)
        {
            //app.UseSwagger();
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((doc, item) =>
                {
                    //根据代理服务器提供的协议、地址和路由，生成api文档服务地址
                    doc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer
                            { Url = $"{item.Scheme}://{item.Host.Value}" }
                    };
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{swaggerOptions.Name}/swagger.json",
                    swaggerOptions.Version);
                c.RoutePrefix = "swagger/api";

                var stream = streamHtml?.Invoke();
                if (stream == null)
                {
                    const string msg = "index.html attribute error";
                    Logger.Error(msg);
                    throw new Exception(msg);
                }

                c.IndexStream = streamHtml;
            });
        }
    }
}

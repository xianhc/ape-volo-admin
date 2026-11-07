/******************************************************************************
 * 项目名称：APE-VOLO-ADMIN
 * 开源地址：https://github.com/xianhc/ape-volo-admin
 * 系统官网：https://www.apevolo.com
 * 联系邮箱：apevolo@gmail.com
 *
 * Copyright © 2022 xianhc（冼浩春）。保留所有权利。
 * SPDX-License-Identifier: Apache-2.0
 *
 * 本项目采用 Apache License 2.0，完整许可条款详见根目录 LICENSE 文件。
 *
 * 版权与署名保留要求（Apache License 2.0 第 4 条）：
 * 再分发本项目或其衍生作品时，须向接收者提供许可证副本。
 * 以源码形式分发衍生作品时，须保留与该作品相关的原始版权及署名通知，
 * 不得删除原始通知或以他人署名替换；修改过的文件须显著注明已作修改。
 * 上述说明为许可证要求的提示，完整义务以 LICENSE 条款为准。
 *
 * 作者保留依法维护自身合法权益的权利。授权与合作事宜请通过上述邮箱联系。
 ******************************************************************************/

using System.Reflection;
using System.Text;
using Ape.Volo.Web.ActionFilters;
using Ape.Volo.Web.DynamicApi;
using Ape.Volo.Web.Middleware;
using Ape.Volo.Common.Converters;
using Ape.Volo.Common.Enums;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Extensions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

// 避免 Windows 默认代码页将版权符号等 Unicode 字符替换为问号。
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// 配置源按环境选择：通用配置打底，环境配置覆盖，环境变量与命令行兜底。
// 环境名取自 ASPNETCORE_ENVIRONMENT（未设置时按 Development），宿主与配置源用同一个值。
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = environment
});
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("ipratelimit.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);
JwtTokenValidation.ValidateOptions(builder.Configuration.GetSection("JwtAuth").Get<JwtAuthOptions>()
                                   ?? new JwtAuthOptions());
// 启动时校验密码传输私钥与 JWT 配置；仓库内保留开发演示密钥。
var passwordPrivateKeyPem = builder.Configuration.ValidateRequiredSecrets(builder.Environment.ContentRootPath);

// 同一数据中心内 WorkerId 必须各不相同，所有 ID 调用共享单例生成器。
builder.Services.AddSnowflakeIdService(builder.Configuration);

// 配置容器
builder.Host
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .UseSerilogMiddleware()
    .ConfigureContainer<ContainerBuilder>(b => { b.RegisterModule(new AutofacExtensions(builder.Configuration)); });


// 配置服务
// IHttpContextAccessor 由 AddHostServices 统一注册；IActionContextAccessor 在 ASP.NET Core 10 已过时且全项目无解析，不再注册
builder.Services.AddMappingService();
builder.Services.AddHostServices(passwordPrivateKeyPem);
builder.Services.AddInfrastructureServices();
//builder.Services.AddAppConfigService(builder.Environment);
builder.Services.AddOptionRegisterService(builder.Configuration);
// builder.Services.Configure<Configs>(configuration);
// var configs = configuration.Get<Configs>();
builder.Services.AddSerilogService();
builder.Services.AddCacheService(builder.Configuration);
builder.Services.AddLogQueueService();
builder.Services.AddSqlSugarService(builder.Configuration);
builder.Services.AddDbService();
builder.Services.AddCorsService(builder.Configuration);
builder.Services.AddMiniProfilerService();
builder.Services.AddSwaggerService(builder.Configuration);
builder.Services.AddQuartzNetJobService();
builder.Services.AddAuthorizationService();
builder.Services.AddBrowserDetection();
builder.Services.AddIpStrategyRateLimitService(builder.Configuration);
var mvcBuilder = builder.Services.AddControllers(options =>
    {
        ValidationMetadataProvider.Configure(options);
        // 授权过滤器
        options.Filters.Add<ServiceAuthorizationExceptionFilter>();
        // 异常过滤器
        options.Filters.Add<ExceptionLogFilter>();
        // 审计过滤器
        options.Filters.Add<OperateLogFilter>();
    })
    .AddControllersAsServices()
    .AddNewtonsoftJson(options =>
        {
            //全局忽略循环引用
            options.AllowInputFormatterExceptionMessages = false;
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
            options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            options.SerializerSettings.ContractResolver = new CustomContractResolver();
        }
    );

// 动态接口：契约层接口按约定映射为端点。
// 必须排在 AddControllersAsServices 之后，否则会给接口补一条无效的 DI 描述符，覆盖 Autofac 的拦截器注册。
mvcBuilder.AddDynamicApi(builder.Configuration);
builder.Services.AddIpSearcherService();

// 配置中间件
var app = builder.Build();

app.AppConfigNotifier();
app.ApplicationStartedNotifier();

// 仅接受受信任代理转发的客户端 IP 和协议。
app.UseForwardedHeaders();
app.UseSerilogRequestLogging(options =>
    options.EnrichDiagnosticContext = (diagnosticContext, context) =>
        diagnosticContext.Set("RequestId", context.TraceIdentifier));
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Request-ID"] = context.TraceIdentifier;
        return Task.CompletedTask;
    });
    await next(context);
});
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// 请求正文缓冲：签名校验与参数读取需要重复读取正文
app.UseMiddleware<RequestBufferingMiddleware>();

//Swagger UI
app.UseSwaggerUiMiddleware(() => Assembly.GetExecutingAssembly().GetManifestResourceStream("Ape.Volo.Web.index.html")
    ?? throw new InvalidOperationException("未找到嵌入的 Swagger 首页资源。"));


//静态文件
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
        context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff"
});
//cookie
app.UseCookiePolicy();
//错误页
app.UseStatusCodePages();
// 路由
app.UseRouting();
//IP限流
app.UseIpLimitMiddleware();
// CORS跨域
app.UseCors(app.Services.GetRequiredService<IOptions<CorsOptions>>().Value.Name);
// 认证
app.UseAuthentication();
app.UseMiddleware<DataScopeMiddleware>();
// 授权
app.UseAuthorization();
//性能监控
app.UseMiniProfilerMiddleware();

//app.UseHttpMethodOverride();

//种子数据
await app.UseDataSeederMiddlewareAsync();

// 动态接口清单：必须排在种子数据之后，避免启动日志写入日志库与建表抢锁
app.UseDynamicApi();

// 注册控制器路由
app.MapControllers();

// 运行
app.Run();

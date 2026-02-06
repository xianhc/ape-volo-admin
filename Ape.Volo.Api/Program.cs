/******************************************************************************
 * 项目名称：APE-VOLO-ADMIN
 * 版权协议：Apache-2.0
 * 开源地址：https://github.com/xianhc/ape-volo-admin
 * 系统官网：https://www.apevolo.com
 * 版权声明：Copyright © 2022 xianhc(冼浩春) 保留所有权利。
 ******************************************************************************/

using System;
using System.Reflection;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Common.IdGenerator.Contract;
using Ape.Volo.Common.MultiLanguage.Resources;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Internal;
using Ape.Volo.Core.Mapping;
using Ape.Volo.Infrastructure.ActionFilter;
using Ape.Volo.Infrastructure.Extensions;
using Ape.Volo.Infrastructure.Middleware;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//配置雪花ID方法参数
IdHelper.SetIdGeneratorOptions(new IdGeneratorOptions(1));

// 配置容器
builder.Host
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        hostingContext.Configuration.ConfigureApplication();
        config.Sources.Clear();
        config.AddJsonFile(builder.Environment.IsDevelopment() ? "appsettings.Development.json" : "appsettings.json",
                optional: true, reloadOnChange: false)
            .AddJsonFile("IpRateLimit.json", optional: true, reloadOnChange: false);
    }).UseSerilogMiddleware()
    .ConfigureContainer<ContainerBuilder>(b => { b.RegisterModule(new AutofacExtensions()); });
builder.ConfigureApplication();


// 配置服务
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<IRegister, CustomMapper>();
builder.Services.AddSingleton<IMapper, Mapper>();
builder.Services.AddSingleton(new AppSettings(builder.Configuration, builder.Environment));
//builder.Services.AddAppConfigService(builder.Environment);
builder.Services.AddOptionRegisterService();
builder.Services.AddCustomMultiLanguagesService();
// builder.Services.Configure<Configs>(configuration);
// var configs = configuration.Get<Configs>();
builder.Services.AddSerilogService();
builder.Services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
builder.Services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });
builder.Services.AddCacheService();
builder.Services.AddSqlSugarService();
builder.Services.AddDbService();
builder.Services.AddCorsService();
builder.Services.AddMiniProfilerService();
builder.Services.AddSwaggerService();
builder.Services.AddQuartzNetJobService();
builder.Services.AddAuthorizationService();
builder.Services.AddBrowserDetection();
builder.Services.AddRedisInitMqService();
builder.Services.AddIpStrategyRateLimitService();
builder.Services.AddRabbitMqService();
builder.Services.AddEventBusService();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 设置会话过期时间
    options.Cookie.HttpOnly = true; // 安全设置，防止客户端脚本访问
    options.Cookie.IsEssential = true; // 确保在没有同意 Cookie 的情况下也能使用
});
builder.Services.AddControllers(options =>
    {
        // 异常过滤器
        options.Filters.Add<ExceptionLogFilter>();
        // 审计过滤器
        options.Filters.Add<OperateLogFilter>();
        if (App.GetOptions<SystemOptions>().RunMode == RunMode.Demo)
        {
            //演示模式
            options.Filters.Add<DemoFilter>();
        }
    })
    //.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(typeof(Language))
    .AddControllersAsServices()
    .AddNewtonsoftJson(options =>
        {
            //全局忽略循环引用
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
            options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            //options.SerializerSettings.ContractResolver = new CustomContractResolver();
        }
    );
builder.Services.AddIpSearcherService();

// 配置中间件
var app = builder.Build();

app.ConfigureApplication();
app.AppConfigNotifier();
app.ApplicationStartedNotifier();

//实体映射配置
var mapper = app.Services.GetRequiredService<IRegister>();
TypeAdapterConfig.GlobalSettings.Apply(mapper);

//多语言请求扩展
app.UseCustomRequestLocalization();

//获取远程真实ip,如果不是nginx代理部署可以不要
app.UseMiddleware<RealIpMiddleware>();
//处理访问不存在的接口
//app.UseMiddleware<NotFoundMiddleware>();
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Use(next => context =>
{
    context.Request.EnableBuffering();
    return next(context);
});

app.UseSession();
// // Swagger Auth
app.UseSwaggerAuthorized();
//Swagger UI
app.UseSwaggerUiMiddleware(() => Assembly.GetExecutingAssembly().GetManifestResourceStream("Ape.Volo.Api.index.html"));


//静态文件
app.UseStaticFiles();
//cookie
app.UseCookiePolicy();
//错误页
app.UseStatusCodePages();
// 路由
app.UseRouting();
//IP限流
app.UseIpLimitMiddleware();
// CORS跨域
app.UseCors(App.GetOptions<CorsOptions>().Name);
// 认证
app.UseAuthentication();
// 授权
app.UseAuthorization();
//性能监控
app.UseMiniProfilerMiddleware();

//app.UseHttpMethodOverride();

//种子数据
app.UseDataSeederMiddleware();

//作业调度
app.UseQuartzNetJobMiddleware();

//事件总线配置订阅
app.ConfigureEventBus();

// 注册控制器路由
app.MapControllers();

// 运行
app.Run();
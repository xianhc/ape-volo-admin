using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Ape.Volo.Infrastructure.Extensions;

public static class AppConfigExtensions
{
    public static void AppConfigNotifier(this WebApplication app)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));

        #region 基础配置信息

        var systemOptions = App.GetOptions<SystemOptions>();
        var baseDict = new Dictionary<string, string>
        {
            {
                "配置文件", App.WebHostEnvironment.IsDevelopment() ? "appsettings.Development.json" : "appsettings.json"
            },
            { "运行模式", systemOptions.RunMode.ToString() },
            { "初始化数据库", systemOptions.IsInitDb.ToString() },
            { "CORS跨域", systemOptions.IsCqrs.ToString() },
            { "默认密码", systemOptions.UserDefaultPassword },
            { "文件上传限制(M)", systemOptions.FileLimitSize.ToString() },
            { "主库ID", systemOptions.MainDataBase },
            { "日志库ID", systemOptions.LogDataBase },
            { "使用Redis缓存", systemOptions.UseRedisCache.ToString() }
        };

        ConsoleHelper.PrintConfigTable(baseDict, " 基础配置信息 ");

        #endregion 基础配置信息

        #region Serilog配置信息

        var serilogOptions = App.GetOptions<SerilogOptions>();
        var serilogDict = new Dictionary<string, string>
        {
            { "记录SQL日志", serilogOptions.RecordSql.ToString() },
            { "写入到数据库", serilogOptions.ToDb.ToString() },
            { "写入到文件", serilogOptions.ToFile.ToString() },
            { "写入到控制台", serilogOptions.ToConsole.ToString() },
            { "写入到ToElasticsearch", serilogOptions.ToElasticsearch.ToString() }
        };
        ConsoleHelper.PrintConfigTable(serilogDict, " Serilog配置信息 ");

        #endregion Serilog配置信息

        #region 中间件配置信息

        var middlewareOptions = App.GetOptions<MiddlewareOptions>();
        var middlewareDict = new Dictionary<string, string>
        {
            { "调度作业", middlewareOptions.QuartzNetJob.ToString() },
            { "IP限流", middlewareOptions.IpLimit.ToString() },
            { "性能监控", middlewareOptions.MiniProfiler.ToString() },
            { "Rabbit消息队列", middlewareOptions.RabbitMq.ToString() },
            { "Redis消息队列", middlewareOptions.RedisMq.ToString() },
            { "Elasticsearch", middlewareOptions.Elasticsearch.ToString() }
        };
        ConsoleHelper.PrintConfigTable(middlewareDict, " 中间件配置信息 ");

        #endregion 中间件配置信息

        #region AOP配置信息

        var aopOptions = App.GetOptions<AopOptions>();
        var aopDict = new Dictionary<string, string>
        {
            { "事务", aopOptions.Transactions.ToString() }, { "缓存", aopOptions.Cache.ToString() }
        };
        ConsoleHelper.PrintConfigTable(aopDict, " AOP配置信息 ");

        #endregion AOP配置信息

        Console.WriteLine();
    }
}
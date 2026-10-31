using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Microsoft.Extensions.Options;
using Ape.Volo.Core.ConfigOptions;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 启动配置输出扩展
/// </summary>
public static class AppConfigExtensions
{
    /// <summary>
    /// 将关键配置项与运行环境信息输出到控制台，便于启动自检
    /// </summary>
    /// <param name="app">应用实例</param>
    /// <exception cref="ArgumentNullException">应用实例为 null</exception>
    public static void AppConfigNotifier(this WebApplication app)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));

        #region 基础配置信息

        var systemOptions = app.Services.GetRequiredService<IOptions<SystemOptions>>().Value;
        var logOptions = app.Services.GetRequiredService<IOptions<LogDataConnectionOptions>>().Value;
        var baseDict = new Dictionary<string, string>
        {
            {
                "配置文件", app.Environment.IsDevelopment() ? "appsettings.Development.json" : "appsettings.json"
            },
            { "运行模式", systemOptions.RunMode.ToString() },
            { "初始化数据库", systemOptions.IsInitDb.ToString() },
            { "文件上传限制(M)", systemOptions.FileLimitSize.ToString() },
            { "主库ID", systemOptions.MainDataBase },
            { "日志库ID", logOptions.ConnId },
            { "使用Redis缓存", systemOptions.UseRedisCache.ToString() }
        };

        ConsoleHelper.PrintConfigTable(baseDict, " 基础配置信息 ");

        #endregion 基础配置信息

        #region Serilog配置信息

        var serilogOptions = app.Services.GetRequiredService<IOptions<SerilogOptions>>().Value;
        var serilogDict = new Dictionary<string, string>
        {
            { "记录SQL日志", serilogOptions.RecordSql.ToString() },
            { "记录SQL参数值", serilogOptions.IncludeSqlParameters.ToString() },
            { "慢SQL阈值(ms)", serilogOptions.SlowSqlThresholdMilliseconds.ToString("0.##") },
            { "写入到数据库", serilogOptions.ToDb.ToString() },
            { "写入到文件", serilogOptions.ToFile.ToString() },
            { "写入到控制台", serilogOptions.ToConsole.ToString() },
            { "写入到ToElasticsearch", serilogOptions.ToElasticsearch.ToString() }
        };
        ConsoleHelper.PrintConfigTable(serilogDict, " Serilog配置信息 ");

        #endregion Serilog配置信息

        #region 中间件配置信息

        var middlewareOptions = app.Services.GetRequiredService<IOptions<MiddlewareOptions>>().Value;
        var middlewareDict = new Dictionary<string, string>
        {
            { "调度作业", middlewareOptions.QuartzNetJob.ToString() },
            { "IP限流", middlewareOptions.IpLimit.ToString() },
            { "性能监控", middlewareOptions.MiniProfiler.ToString() },
            { "Elasticsearch", middlewareOptions.Elasticsearch.ToString() }
        };
        ConsoleHelper.PrintConfigTable(middlewareDict, " 中间件配置信息 ");

        #endregion 中间件配置信息

        #region AOP配置信息

        var aopOptions = app.Services.GetRequiredService<IOptions<AopOptions>>().Value;
        var aopDict = new Dictionary<string, string>
        {
            { "事务", aopOptions.Transactions.ToString() }, { "缓存", aopOptions.Cache.ToString() }
        };
        ConsoleHelper.PrintConfigTable(aopDict, " AOP配置信息 ");

        #endregion AOP配置信息

        Console.WriteLine();
    }
}

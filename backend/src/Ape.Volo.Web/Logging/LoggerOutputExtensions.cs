using Ape.Volo.Core.Serilog;
using Serilog;
using Serilog.Events;
using Elastic.Serilog.Sinks;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Transport;
using Ape.Volo.Core.ConfigOptions;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 宿主日志输出端配置，与日志批次分类实现分离。
/// </summary>
public static class LoggerOutputExtensions
{
    /// <summary>
    /// 控制台
    /// </summary>
    /// <param name="loggerConfiguration"></param>
    /// <returns></returns>
    public static LoggerConfiguration WriteToConsole(this LoggerConfiguration loggerConfiguration)
    {
        //系统日志
        loggerConfiguration = loggerConfiguration.WriteTo.Logger(lg =>
            lg.RecordLog().WriteTo.Console());

        //sql日志
        loggerConfiguration = loggerConfiguration.WriteTo.Logger(lg =>
            lg.RecordSql(LoggerProperty.ToConsole).WriteTo.Console());

        return loggerConfiguration;
    }

    /// <summary>
    /// 文件
    /// </summary>
    /// <param name="loggerConfiguration"></param>
    /// <param name="contentRootPath">内容根目录，日志写入其下的 logs/日期 子目录</param>
    /// <returns></returns>
    public static LoggerConfiguration WriteToFile(this LoggerConfiguration loggerConfiguration, string contentRootPath)
    {
        //系统日志
        foreach (var level in Enum.GetValues<LogEventLevel>())
        {
            loggerConfiguration.WriteTo.Logger(lg =>
                lg.Filter.ByIncludingOnly(p => p.Level == level).RecordLog().WriteTo.Async(s =>
                    s.Sink(new DailyFileSink(contentRootPath, level + ".log"))));
        }

        //sql日志
        loggerConfiguration = loggerConfiguration.WriteTo.Logger(lg =>
            lg.RecordSql(LoggerProperty.ToFile)
                .WriteTo.Async(s =>
                    s.Sink(new DailyFileSink(contentRootPath, "AopSql.log"))));
        return loggerConfiguration;
    }

    /// <summary>
    /// Elasticsearch
    /// </summary>
    /// <param name="loggerConfiguration"></param>
    /// <returns></returns>
    /// <param name="settings">节点地址和 API Key 配置。</param>
    public static LoggerConfiguration WriteToElasticsearch(this LoggerConfiguration loggerConfiguration, SerilogOptions settings)
    {
        //系统日志
        loggerConfiguration = loggerConfiguration.WriteTo.Logger(lg =>
            lg.RecordLog().WriteTo.Elasticsearch([new Uri(settings.ElasticsearchEndpoint)], options =>
                {
                    options.DataStream = new DataStreamName("logs", "ape-volo", "application");
                    options.BootstrapMethod = BootstrapMethod.Failure;
                }, transport =>
                {
                    if (!string.IsNullOrWhiteSpace(settings.ElasticsearchApiKey))
                        transport.Authentication(new ApiKey(settings.ElasticsearchApiKey));
                }));

        //sql日志
        loggerConfiguration = loggerConfiguration.WriteTo.Logger(lg =>
            lg.RecordSql(LoggerProperty.ToElasticsearch)
                .WriteTo.Elasticsearch([new Uri(settings.ElasticsearchEndpoint)], options =>
                {
                    options.DataStream = new DataStreamName("logs", "ape-volo", "sql");
                    options.BootstrapMethod = BootstrapMethod.Failure;
                }, transport =>
                {
                    if (!string.IsNullOrWhiteSpace(settings.ElasticsearchApiKey))
                        transport.Authentication(new ApiKey(settings.ElasticsearchApiKey));
                }));

        return loggerConfiguration;
    }


}

using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// 中间件配置
/// </summary>
[OptionsSettings]
public class MiddlewareOptions
{
    /// <summary>
    /// 作业调度
    /// </summary>
    public bool QuartzNetJob { get; set; }

    /// <summary>
    /// Ip限流
    /// </summary>
    public bool IpLimit { get; set; }

    /// <summary>
    /// 性能监控
    /// </summary>
    public bool MiniProfiler { get; set; }

    /// <summary>
    /// Rabbit消息队列
    /// </summary>
    public bool RabbitMq { get; set; }

    /// <summary>
    /// Redis消息队列
    /// </summary>
    public bool RedisMq { get; set; }

    /// <summary>
    /// Elasticsearch日志收集
    /// </summary>
    public bool Elasticsearch { get; set; }
}
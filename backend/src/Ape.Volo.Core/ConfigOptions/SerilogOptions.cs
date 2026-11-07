using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// Serilog日志配置
/// </summary>
[OptionsSettings]
public class SerilogOptions
{
    /// <summary>控制台日志前缀，不包含方括号。</summary>
    public string Prefix { get; set; } = "APE-VOLO";

    /// <summary>
    /// 是否记录SQL日志
    /// </summary>
    public bool RecordSql { get; set; }

    /// <summary>
    /// 是否在 SQL 日志中展开参数值；生产环境建议关闭。
    /// </summary>
    public bool IncludeSqlParameters { get; set; }

    /// <summary>
    /// 慢 SQL 阈值（毫秒）。
    /// </summary>
    public double SlowSqlThresholdMilliseconds { get; set; } = 300;

    /// <summary>
    /// 输出到数据库
    /// </summary>
    public bool ToDb { get; set; }

    /// <summary>
    /// 输出到文件
    /// </summary>
    public bool ToFile { get; set; }

    /// <summary>
    /// 输出到控制台
    /// </summary>
    public bool ToConsole { get; set; }

    /// <summary>
    /// 输出到Elasticsearch
    /// </summary>
    public bool ToElasticsearch { get; set; }

    /// <summary>Elasticsearch 8.15 及以上的节点地址。</summary>
    public string ElasticsearchEndpoint { get; set; } = "http://127.0.0.1:9200";

    /// <summary>Elasticsearch API Key；通过环境变量配置，空值用于无认证的本地实例。</summary>
    public string ElasticsearchApiKey { get; set; } = string.Empty;
}

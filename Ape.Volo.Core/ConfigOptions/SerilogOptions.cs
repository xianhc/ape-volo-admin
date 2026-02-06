using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// Serilog日志配置
/// </summary>
[OptionsSettings]
public class SerilogOptions
{
    /// <summary>
    /// 是否记录SQL日志
    /// </summary>
    public bool RecordSql { get; set; }

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
}
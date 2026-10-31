using Ape.Volo.Common.Attributes;
using SqlSugar;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// 数据库连接配置（业务库）
/// </summary>
[OptionsSettings]
public class DataConnectionOptions
{
    /// <summary>
    /// 列表
    /// </summary>
    public List<ConnectionItem> ConnectionItem { get; set; } = new();
}

/// <summary>
/// 日志库连接配置
/// </summary>
[OptionsSettings]
public class LogDataConnectionOptions : ConnectionItem
{
}

/// <summary>
/// 连接符
/// </summary>
public class ConnectionItem
{
    /// <summary>
    /// 连接ID（唯一）
    /// </summary>
    public string ConnId { get; set; } = string.Empty;

    /// <summary>
    /// 数据库类型
    /// </summary>
    public DbType DbType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}

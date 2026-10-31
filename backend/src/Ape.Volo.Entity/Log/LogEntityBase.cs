using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Log;

/// <summary>
/// 日志实体基类
/// 只带创建审计，日志是追加写入的记录，不参与数据权限、也不做逻辑删除
/// </summary>
public abstract class LogEntityBase : EntityBase, ICreationAuditedEntity
{
    /// <summary>
    /// 创建者主键；后台任务或匿名写入时为空
    /// </summary>
    [SugarColumn(IsOnlyIgnoreUpdate = true)]
    public long? CreatorId { get; set; }

    /// <summary>
    /// 创建时间（分表字段）
    /// </summary>
    [SplitField]
    [SugarColumn(IsOnlyIgnoreUpdate = true)]
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Message { get; set; }

    /// <summary>
    /// 消息模板
    /// </summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? MessageTemplate { get; set; }

    /// <summary>
    /// 属性
    /// </summary>
    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Properties { get; set; }
}

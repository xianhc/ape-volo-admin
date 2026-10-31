using SqlSugar;

namespace Ape.Volo.Entity.Base;

/// <summary>
/// 审计实体基类
/// 只补齐审计字段与软删除能力，不参与数据权限
/// </summary>
[SugarIndex("index_{table}_CreatorId", nameof(CreatorId), OrderByType.Asc)]
public abstract class AuditedEntity : EntityBase, IAuditedEntity, ISoftDeletedEntity
{
    /// <summary>
    /// 创建者主键；后台任务或匿名写入时为空
    /// </summary>
    [SugarColumn(IsOnlyIgnoreUpdate = true)]
    public long? CreatorId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(IsOnlyIgnoreUpdate = true)]
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 最后修改者主键；后台任务或匿名写入时为空
    /// </summary>
    [SugarColumn(IsOnlyIgnoreInsert = true)]
    public long? LastModifierId { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空
    /// </summary>
    [SugarColumn(IsOnlyIgnoreInsert = true)]
    public DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// 是否已逻辑删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 执行删除者主键；未删除或恢复后为空
    /// </summary>
    public long? DeleterId { get; set; }

    /// <summary>
    /// 删除时间；未删除或恢复后为空
    /// </summary>
    public DateTime? DeletionTime { get; set; }
}

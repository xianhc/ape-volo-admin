namespace Ape.Volo.Entity.Base;

/// <summary>
/// 修改审计契约
/// </summary>
public interface IModificationAuditedEntity
{
    /// <summary>
    /// 最后修改者主键；后台任务或匿名写入时为空
    /// </summary>
    long? LastModifierId { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空
    /// </summary>
    DateTime? LastModificationTime { get; set; }
}

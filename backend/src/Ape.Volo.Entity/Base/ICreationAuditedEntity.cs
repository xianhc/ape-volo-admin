namespace Ape.Volo.Entity.Base;

/// <summary>
/// 创建审计契约
/// </summary>
public interface ICreationAuditedEntity
{
    /// <summary>
    /// 创建者主键；后台任务或匿名写入时为空
    /// </summary>
    long? CreatorId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime CreationTime { get; set; }
}

namespace Ape.Volo.Entity.Base;

/// <summary>
/// 软删除契约：标记位与删除审计
/// </summary>
public interface ISoftDeletedEntity
{
    /// <summary>
    /// 是否已逻辑删除
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// 执行删除者主键；未删除或恢复后为空
    /// </summary>
    long? DeleterId { get; set; }

    /// <summary>
    /// 删除时间；未删除或恢复后为空
    /// </summary>
    DateTime? DeletionTime { get; set; }
}

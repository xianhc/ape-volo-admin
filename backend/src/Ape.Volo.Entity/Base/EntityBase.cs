using SqlSugar;

namespace Ape.Volo.Entity.Base;

/// <summary>
/// 实体基类
/// 只提供主键，不携带任何审计或策略语义
/// </summary>
public abstract class EntityBase : IEntity<long>
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public long Id { get; set; }
}

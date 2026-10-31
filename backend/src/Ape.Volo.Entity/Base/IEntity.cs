namespace Ape.Volo.Entity.Base;

/// <summary>
/// 主键契约
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IEntity<TKey>
{
    /// <summary>
    /// 主键
    /// </summary>
    TKey Id { get; set; }
}

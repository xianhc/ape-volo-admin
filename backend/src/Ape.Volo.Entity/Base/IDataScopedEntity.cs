namespace Ape.Volo.Entity.Base;

/// <summary>
/// 数据权限参与契约
/// 空标记接口：只有实现它的实体才会被数据权限过滤器按创建者过滤，
/// 因此「是否参与数据权限」由接口表达，而不是由继承哪个基类表达。
/// </summary>
public interface IDataScopedEntity
{
    /// <summary>
    /// 创建者主键，数据权限按此列比对
    /// </summary>
    long? CreatorId { get; set; }
}

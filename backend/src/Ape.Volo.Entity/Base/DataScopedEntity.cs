namespace Ape.Volo.Entity.Base;

/// <summary>
/// 数据权限实体基类
/// 与 <see cref="AuditedEntity"/> 相比不新增任何字段，只声明参与数据权限过滤
/// </summary>
public abstract class DataScopedEntity : AuditedEntity, IDataScopedEntity
{
}

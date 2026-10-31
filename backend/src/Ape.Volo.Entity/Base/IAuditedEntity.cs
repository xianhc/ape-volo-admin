namespace Ape.Volo.Entity.Base;

/// <summary>
/// 创建与修改审计的组合契约
/// </summary>
public interface IAuditedEntity : ICreationAuditedEntity, IModificationAuditedEntity
{
}

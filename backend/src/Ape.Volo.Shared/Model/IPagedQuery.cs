namespace Ape.Volo.Shared.Model;

/// <summary>
/// 分页查询条件：同时提供分页参数与查询条件。
/// 查询条件由 <see cref="Ape.Volo.Shared.Attributes.QueryConditionAttribute"/> 标注的属性描述，
/// 契约层不绑定任何 ORM 类型。
/// </summary>
public interface IPagedQuery : IPagedAndSortedResultRequest
{
}

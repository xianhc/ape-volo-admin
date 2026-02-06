using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using Newtonsoft.Json;

namespace Ape.Volo.ViewModel.Core.Permission.Department;

/// <summary>
/// 部门Vo
/// </summary>
[AutoMapping(typeof(Entity.Core.Permission.Department), typeof(DepartmentVo))]
public class DepartmentVo : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 父级Id
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<DepartmentVo> Children { get; set; }

    /// <summary>
    /// 子节点个数
    /// </summary>
    public int SubCount { get; set; }

    /// <summary>
    /// 是否有子节点
    /// </summary>
    public bool HasChildren => SubCount > 0;

    /// <summary>
    /// 页
    /// </summary>
    public bool Leaf => SubCount == 0;
}

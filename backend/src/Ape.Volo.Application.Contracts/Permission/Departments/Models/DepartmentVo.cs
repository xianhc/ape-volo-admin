using Newtonsoft.Json;

namespace Ape.Volo.Application.Contracts.Permission.Departments.Models;

/// <summary>
/// 部门Vo
/// </summary>
public class DepartmentVo : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父级Id（null 表示顶级部门）
    /// </summary>
    public long? ParentId { get; set; }

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
    public List<DepartmentVo> Children { get; set; } = new();

    /// <summary>
    /// 是否有子节点，供懒加载树表判断是否显示展开箭头
    /// </summary>
    public bool HasChildren { get; set; }
}

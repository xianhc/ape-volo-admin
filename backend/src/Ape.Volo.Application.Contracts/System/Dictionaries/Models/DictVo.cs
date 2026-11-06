using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.System.Dictionaries.Models;

/// <summary>
/// 字典Vo
/// </summary>
public class DictVo : BaseEntityDto<long>
{
    /// <summary>
    /// 字典类型
    /// </summary>
    /// <returns></returns>
    public DictType DictType { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 字典详情
    /// </summary>
    public List<DictDetailVo> DictDetails { get; set; } = new();
}

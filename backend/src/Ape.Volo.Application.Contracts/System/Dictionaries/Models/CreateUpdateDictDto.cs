using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.System.Dictionaries.Models;

/// <summary>
/// 字典Dto
/// </summary>
public sealed record CreateUpdateDictDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 字典类型
    /// </summary>
    [Display(Name = "字典类型")]
    [Range(1, 2, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public DictType DictType { get; init; }

    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "字典名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "字典描述")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Description { get; init; } = string.Empty;
}

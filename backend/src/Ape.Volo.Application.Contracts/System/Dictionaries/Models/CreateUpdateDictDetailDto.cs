using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.System.Dictionaries.Models;

/// <summary>
/// 字典详情Dto
/// </summary>
public sealed record CreateUpdateDictDetailDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 字典ID
    /// </summary>
    [Display(Name = "字典ID")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public long DictId { get; init; }

    /// <summary>
    /// 标签
    /// </summary>
    [Display(Name = "字典标签")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    [Display(Name = "字典值")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "排序")]
    [Range(1, 999, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public int DictSort { get; init; }
}

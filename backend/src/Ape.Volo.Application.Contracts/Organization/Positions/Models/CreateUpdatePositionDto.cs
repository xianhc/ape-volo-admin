using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Organization.Positions.Models;

/// <summary>
/// 岗位Dto
/// </summary>
public sealed record CreateUpdatePositionDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "岗位名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "排序")]
    [Range(1, 999, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public int Sort { get; init; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; init; }
}

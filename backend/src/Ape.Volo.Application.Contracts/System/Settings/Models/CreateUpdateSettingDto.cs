using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.System.Settings.Models;

/// <summary>
/// 系统配置Dto
/// </summary>
public sealed record CreateUpdateSettingDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 键
    /// </summary>
    [Display(Name = "参数名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    [Display(Name = "参数值")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "描述")]
    public string? Description { get; init; }
}
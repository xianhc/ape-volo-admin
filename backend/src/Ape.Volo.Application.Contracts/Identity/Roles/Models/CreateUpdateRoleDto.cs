using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Identity.Roles.Models;

/// <summary>
/// 角色Dto
/// </summary>
public sealed record CreateUpdateRoleDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "描述")]
    public string? Description { get; init; }

    /// <summary>
    /// 数据权限
    /// </summary>
    [Display(Name = "数据权限类型")]
    [Range(0, 5, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public DataScopeType DataScopeType { get; init; }

    /// <summary>
    /// 标识
    /// </summary>
    [Display(Name = "权限标识")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string AuthCode { get; init; } = string.Empty;

    /// <summary>
    /// 部门ID数组
    /// </summary>
    public List<long> DeptIdArray { get; init; } = new();
}

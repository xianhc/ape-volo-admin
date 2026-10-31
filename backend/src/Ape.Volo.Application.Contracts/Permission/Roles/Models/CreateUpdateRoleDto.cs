using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Permission.Roles.Models;

/// <summary>
/// 角色Dto
/// </summary>
public class CreateUpdateRoleDto : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "名称")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 数据权限
    /// </summary>
    [Display(Name = "数据权限类型")]
    [Range(0, 5, ErrorMessage = "{0}range{1}{2}")]
    public DataScopeType DataScopeType { get; set; }

    /// <summary>
    /// 标识
    /// </summary>
    [Display(Name = "权限标识")]
    [Required(ErrorMessage = "{0}required")]
    public string AuthCode { get; set; } = string.Empty;

    /// <summary>
    /// 部门ID数组
    /// </summary>
    public List<long> DeptIdArray { get; set; } = new();
}

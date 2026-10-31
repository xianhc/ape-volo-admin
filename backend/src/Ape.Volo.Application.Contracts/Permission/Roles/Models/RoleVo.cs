using Ape.Volo.Application.Contracts.Permission.Departments.Models;
using Ape.Volo.Application.Contracts.Permission.Menus.Models;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Permission.Roles.Models;

/// <summary>
/// 角色Vo
/// </summary>
public class RoleVo : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 数据权限
    /// </summary>
    public DataScopeType DataScopeType { get; set; }

    /// <summary>
    /// 权限标识
    /// </summary>
    public string AuthCode { get; set; } = string.Empty;

    /// <summary>
    /// 菜单列表
    /// </summary>
    public List<MenuVo> MenuList { get; set; } = new();

    /// <summary>
    /// 部门列表
    /// </summary>
    public List<DepartmentVo> DepartmentList { get; set; } = new();
}

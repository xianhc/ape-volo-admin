using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.Permission;
using Ape.Volo.ViewModel.Core.Permission.Department;
using Ape.Volo.ViewModel.Core.Permission.Menu;

namespace Ape.Volo.ViewModel.Core.Permission.Role;

/// <summary>
/// 角色Vo
/// </summary>
[AutoMapping(typeof(Entity.Core.Permission.Role.Role), typeof(RoleVo))]
public class RoleVo : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 数据权限
    /// </summary>
    public DataScopeType DataScopeType { get; set; }

    /// <summary>
    /// 权限标识
    /// </summary>
    public string AuthCode { get; set; }

    /// <summary>
    /// 菜单列表
    /// </summary>
    public List<MenuVo> MenuList { get; set; }

    /// <summary>
    /// 部门列表
    /// </summary>
    public List<DepartmentVo> DepartmentList { get; set; }

    /// <summary>
    /// 菜单列表
    /// </summary>
    public List<Apis> ApiList { get; set; }
}

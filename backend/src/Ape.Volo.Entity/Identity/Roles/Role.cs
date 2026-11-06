using Ape.Volo.Entity.Organization.Departments;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Identity.Users;
using Ape.Volo.Shared.Enums;
using SqlSugar;

namespace Ape.Volo.Entity.Identity.Roles
{
    /// <summary>
    /// 角色
    /// </summary>
    [SugarTable("sys_role")]
    [SugarIndex("unique_{table}_Name", nameof(Name), OrderByType.Asc, true)]
    [SugarIndex("unique_{table}_AuthCode", nameof(AuthCode), OrderByType.Asc, true)]
    public class Role : DataScopedEntity
    {
        /// <summary>
        /// 角色名称
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
        /// 角色代码
        /// </summary>
        public string AuthCode { get; set; } = string.Empty;

        /// <summary>
        /// 菜单集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(typeof(RoleMenu), nameof(RoleMenu.RoleId), nameof(RoleMenu.MenuId))]
        public List<Menu>? MenuList { get; set; }

        /// <summary>
        /// 部门集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(typeof(RoleDepartment), nameof(RoleDepartment.RoleId), nameof(RoleDepartment.DeptId))]
        public List<Department>? DepartmentList { get; set; }

        /// <summary>
        /// 用户列表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(typeof(UserRole), nameof(UserRole.RoleId), nameof(UserRole.UserId))]
        public List<User>? Users { get; set; }
    }
}

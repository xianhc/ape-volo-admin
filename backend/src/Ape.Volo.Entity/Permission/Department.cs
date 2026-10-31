using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Permission.Roles;
using Ape.Volo.Entity.Permission.Users;
using SqlSugar;

namespace Ape.Volo.Entity.Permission
{
    /// <summary>
    /// 部门
    /// </summary>
    [SugarTable("sys_department")]
    [SugarIndex("index_{table}_ParentId", nameof(ParentId), OrderByType.Asc)]
    public class Department : DataScopedEntity
    {
        /// <summary>
        /// 部门名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 父级部门ID（null 表示顶级部门）
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(DefaultValue = "999")]
        public int Sort { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 用户列表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(NavigateType.OneToMany, nameof(User.DeptId), nameof(Id))]
        public List<User>? Users { get; set; }

        /// <summary>
        /// 用户集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(typeof(RoleDepartment), nameof(RoleDepartment.DeptId), nameof(RoleDepartment.RoleId))]
        public List<Role>? Roles { get; set; }
    }
}

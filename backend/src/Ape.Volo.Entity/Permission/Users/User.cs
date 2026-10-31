using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Organization;
using Ape.Volo.Entity.Permission.Roles;
using Ape.Volo.Shared.Enums;
using SqlSugar;

namespace Ape.Volo.Entity.Permission.Users
{
    /// <summary>
    /// 系统用户
    /// </summary>
    [SugarTable("sys_user")]
    [SugarIndex("unique_{table}_UserName", nameof(UserName), OrderByType.Asc, true)]
    [SugarIndex("unique_{table}_Email", nameof(Email), OrderByType.Asc, true)]
    [SugarIndex("index_{table}_Phone", nameof(Phone), OrderByType.Asc)]
    [SugarIndex("index_{table}_DeptId", nameof(DeptId), OrderByType.Asc)]
    public class User : DataScopedEntity
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 昵称
        /// </summary>
        public string? NickName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 部门
        /// </summary>
        public long DeptId { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        [SugarColumn(Length = 11)]
        public string? Phone { get; set; }

        /// <summary>
        /// 头像路径
        /// </summary>
        public string? AvatarPath { get; set; }

        /// <summary>
        /// 最后修改密码时间
        /// </summary>
        public DateTime? PasswordReSetTime { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public GenderCode GenderCode { get; set; }


        /// <summary>
        /// 界面偏好配置
        /// </summary>
        [SugarColumn(Length = 2000)]
        public string? PreferencesConfig { get; set; }


        #region 扩展属性

        /// <summary>
        /// 部门
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(NavigateType.OneToOne, nameof(DeptId))]
        public Department? Dept { get; set; }

        /// <summary>
        /// 角色集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(typeof(UserRole), nameof(UserRole.UserId), nameof(UserRole.RoleId))]
        public List<Role>? Roles { get; set; }

        /// <summary>
        /// 岗位集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(typeof(UserPosition), nameof(UserPosition.UserId), nameof(UserPosition.PositionId))]
        public List<Position>? Positions { get; set; }

        #endregion
    }
}

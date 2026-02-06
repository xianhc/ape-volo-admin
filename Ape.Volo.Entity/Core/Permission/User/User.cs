using System;
using System.Collections.Generic;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.System;
using SqlSugar;

namespace Ape.Volo.Entity.Core.Permission.User
{
    /// <summary>
    /// 系统用户
    /// </summary>
    [SugarTable("sys_user")]
    [SugarIndex("unique_{table}_UserName", nameof(UserName), OrderByType.Asc, true)]
    public class User : BaseEntity
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
        /// 租户ID
        /// </summary>
        public int? TenantId { get; set; }


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
        public List<Role.Role>? Roles { get; set; }

        /// <summary>
        /// 岗位集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(typeof(UserJob), nameof(UserJob.UserId), nameof(UserJob.JobId))]
        public List<Job>? Jobs { get; set; }


        /// <summary>
        /// 租户
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(NavigateType.OneToOne, nameof(Tenant.TenantId), nameof(TenantId))]
        public Tenant? Tenant { get; set; }

        #endregion
    }
}
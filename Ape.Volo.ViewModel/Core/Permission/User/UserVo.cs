using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using Ape.Volo.ViewModel.Core.Permission.Department;
using Ape.Volo.ViewModel.Core.Permission.Job;
using Ape.Volo.ViewModel.Core.Permission.Role;
using Ape.Volo.ViewModel.Core.System;
using Newtonsoft.Json;

namespace Ape.Volo.ViewModel.Core.Permission.User;

/// <summary>
/// 用户Vo
/// </summary>
[AutoMapping(typeof(Entity.Core.Permission.User.User), typeof(UserVo))]
public class UserVo : BaseEntityDto<long>
{
    /// <summary>
    /// 
    /// </summary>
    public UserVo()
    {
        Roles = new List<RoleSmallVo>();
        Jobs = new List<JobSmallDto>();
    }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [JsonIgnore]
    public string Password { get; set; }

    /// <summary>
    /// 部门
    /// </summary>
    public long DeptId { get; set; }

    /// <summary>
    /// 电话
    /// </summary>
    public string Phone { get; set; }


    /// <summary>
    /// 头像文件路径
    /// </summary>
    [JsonProperty("headerImg")]
    public string AvatarPath { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderCode GenderCode { get; set; }

    /// <summary>
    /// 最后修改密码时间
    /// </summary>
    public DateTime? PasswordReSetTime { get; set; }

    /// <summary>
    /// 角色列表
    /// </summary>
    public List<RoleSmallVo> Roles { get; set; }

    /// <summary>
    /// 部门
    /// </summary>
    public DepartmentSmallVo Dept { get; set; }

    /// <summary>
    /// 岗位列表
    /// </summary>
    public List<JobSmallDto> Jobs { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public int? TenantId { get; set; }

    /// <summary>
    /// 租户
    /// </summary>
    public TenantVo Tenant { get; set; }
}
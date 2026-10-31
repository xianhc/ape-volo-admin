using Ape.Volo.Application.Contracts.Organization.Models;
using Ape.Volo.Application.Contracts.Permission.Departments.Models;
using Ape.Volo.Application.Contracts.Permission.Roles.Models;
using Ape.Volo.Shared.Enums;
using Newtonsoft.Json;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 用户Vo
/// </summary>
public class UserVo : BaseEntityDto<long>
{
    /// <summary>
    /// 
    /// </summary>
    public UserVo()
    {
        Roles = new List<RoleSmallVo>();
        Positions = new List<PositionSmallVo>();
    }

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
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 部门
    /// </summary>
    public long DeptId { get; set; }

    /// <summary>
    /// 电话
    /// </summary>
    public string? Phone { get; set; }


    /// <summary>
    /// 头像文件路径
    /// </summary>
    [JsonProperty("headerImg")]
    public string? AvatarPath { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderCode GenderCode { get; set; }

    /// <summary>
    /// 最后修改密码时间
    /// </summary>
    public DateTime? PasswordReSetTime { get; set; }

    /// <summary>
    /// 界面偏好配置
    /// </summary>
    public string? PreferencesConfig { get; set; }

    /// <summary>
    /// 角色列表
    /// </summary>
    public List<RoleSmallVo> Roles { get; set; } = new();

    /// <summary>
    /// 部门
    /// </summary>
    public DepartmentSmallVo? Dept { get; set; }

    /// <summary>
    /// 岗位列表
    /// </summary>
    public List<PositionSmallVo> Positions { get; set; } = new();
}

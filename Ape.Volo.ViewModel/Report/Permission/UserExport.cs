using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Model;

namespace Ape.Volo.ViewModel.Report.Permission;

/// <summary>
/// 用户导出模板
/// </summary>
[Display(Name = "User.Username")]
public class UserExport : ExportBase
{
    /// <summary>
    /// 用户名称
    /// </summary>
    [Display(Name = "User.Username")]
    public string Username { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    [Display(Name = "Role.Name")]
    public string Role { get; set; }

    /// <summary>
    /// 用户昵称
    /// </summary>
    [Display(Name = "User.NickName")]
    public string NickName { get; set; }

    /// <summary>
    /// 用户电话
    /// </summary>
    [Display(Name = "Sys.Phone")]
    public string Phone { get; set; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    [Display(Name = "Sys.Email")]
    public string Email { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    [Display(Name = "User.Enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    [Display(Name = "Dept.Name")]
    public string Dept { get; set; }

    /// <summary>
    /// 岗位名称
    /// </summary>
    [Display(Name = "Job.Name")]
    public string Job { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [Display(Name = "User.Gender")]
    public GenderCode GenderCode { get; set; }
}

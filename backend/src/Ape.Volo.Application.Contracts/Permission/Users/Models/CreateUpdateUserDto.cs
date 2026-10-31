using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 用户Dto
/// </summary>
public class CreateUpdateUserDto : BaseEntityDto<long>
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名称")]
    [Required(ErrorMessage = "{0}required")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 昵称
    /// </summary>
    [Display(Name = "用户昵称")]
    [Required(ErrorMessage = "{0}required")]
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Display(Name = "邮箱")]
    [Required(ErrorMessage = "{0}required")]
    [EmailAddress(ErrorMessage = "{0}Error.Format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 电话
    /// </summary>
    [Display(Name = "电话")]
    [RegularExpression(@"^(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\d{8}$",
        ErrorMessage = "{0}Error.Format")]
    public string? Phone { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [Display(Name = "性别")]
    [Required(ErrorMessage = "{0}required")]
    public GenderCode GenderCode { get; set; }

    /// <summary>
    /// 部门
    /// </summary>
    [Display(Name = "部门")]
    [Required(ErrorMessage = "{0}required")]
    public UserDeptDto Dept { get; set; } = default!;

    /// <summary>
    /// 角色
    /// </summary>
    [Display(Name = "角色")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public List<UserRoleDto> Roles { get; set; } = new();

    /// <summary>
    /// 岗位
    /// </summary>
    [Display(Name = "岗位")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public List<UserPositionDto> Positions { get; set; } = new();
}

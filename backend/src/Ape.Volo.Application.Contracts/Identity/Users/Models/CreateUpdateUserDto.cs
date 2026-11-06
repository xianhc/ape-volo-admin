using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Identity.Users.Models;

/// <summary>
/// 用户Dto
/// </summary>
public sealed record CreateUpdateUserDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// 昵称
    /// </summary>
    [Display(Name = "用户昵称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string NickName { get; init; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Display(Name = "邮箱")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [EmailAddress(ErrorMessage = "{0}格式不正确。")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// 电话
    /// </summary>
    [Display(Name = "电话")]
    [RegularExpression(@"^(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\d{8}$",
        ErrorMessage = "{0}格式不正确。")]
    public string? Phone { get; init; }

    /// <summary>
    /// 性别
    /// </summary>
    [Display(Name = "性别")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public GenderCode GenderCode { get; init; }

    /// <summary>
    /// 部门
    /// </summary>
    [Display(Name = "部门")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public UserDeptDto Dept { get; init; } = default!;

    /// <summary>
    /// 角色
    /// </summary>
    [Display(Name = "角色")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [AtLeastOneItem]
    public List<UserRoleDto> Roles { get; init; } = new();

    /// <summary>
    /// 岗位
    /// </summary>
    [Display(Name = "岗位")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [AtLeastOneItem]
    public List<UserPositionDto> Positions { get; init; } = new();
}

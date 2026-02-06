using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;

namespace Ape.Volo.SharedModel.Dto.Core.Permission.User;

/// <summary>
/// 修改用户角色
/// </summary>
public class UpdateUserRole
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [Display(Name = "Sys.Id")]
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }

    /// <summary>
    /// 角色Id数组
    /// </summary>
    [Display(Name = "Sys.Role")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public List<long> RoleIdArray { get; set; }
}

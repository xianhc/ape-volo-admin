using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Attributes;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 
/// </summary>
public class UpdateUserPositionDto
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [Display(Name = "ID")]
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }

    /// <summary>
    /// 角色Id数组
    /// </summary>
    [Display(Name = "岗位")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public List<long> PositionIdArray { get; set; } = new();
}

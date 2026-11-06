using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Attributes;

namespace Ape.Volo.Application.Contracts.Identity.Users.Models;

/// <summary>
/// 
/// </summary>
public sealed record UpdateUserPositionDto
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [Display(Name = "ID")]
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; init; }

    /// <summary>
    /// 角色Id数组
    /// </summary>
    [Display(Name = "岗位")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [AtLeastOneItem]
    public List<long> PositionIdArray { get; init; } = new();
}

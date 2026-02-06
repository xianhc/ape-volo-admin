using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;

namespace Ape.Volo.SharedModel.Dto.Core.Permission.Role;

/// <summary>
/// 更新角色Api
/// </summary>
public class UpdateRoleApiDto
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [Display(Name = "Sys.ID")]
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }

    /// <summary>
    /// 角色Api
    /// </summary>
    [Display(Name = "Sys.Api")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public List<long> ApiIdArray { get; set; }
}

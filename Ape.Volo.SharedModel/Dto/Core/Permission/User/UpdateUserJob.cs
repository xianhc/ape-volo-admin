using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;

namespace Ape.Volo.SharedModel.Dto.Core.Permission.User;

/// <summary>
/// 
/// </summary>
public class UpdateUserJob
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
    [Display(Name = "Sys.Job")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public List<long> JobIdArray { get; set; }
}

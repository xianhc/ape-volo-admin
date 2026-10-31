using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 用户部门Dto
/// </summary>
public class UserDeptDto
{
    /// <summary>
    /// ID
    /// </summary>
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Core.System;

namespace Ape.Volo.SharedModel.Dto.Core.Permission.User;

/// <summary>
/// 用户租户Dto
/// </summary>
[AutoMapping(typeof(Tenant), typeof(UserTenantDto))]
public class UserTenantDto
{
    /// <summary>
    /// ID
    /// </summary>
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }
}

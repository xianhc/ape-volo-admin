using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.System;
using SqlSugar;

namespace Ape.Volo.SharedModel.Dto.Core.System;

/// <summary>
/// 
/// </summary>
[AutoMapping(typeof(Tenant), typeof(CreateUpdateTenantDto))]
public class CreateUpdateTenantDto : BaseEntityDto<long>
{
    /// <summary>
    /// 租户Id 用户绑定用的
    /// </summary>
    [Display(Name = "Tenant.TenantId")]
    public int TenantId { get; set; }

    /// <summary>
    /// 标识Id 获取租户连接用的
    /// </summary>
    [Display(Name = "Tenant.ConfigId")]
    public string ConfigId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "Tenant.Name")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    [Display(Name = "Tenant.TenantType")]
    [Range(1, 2, ErrorMessage = "{0}range{1}{2}")]
    public TenantType TenantType { get; set; }

    /// <summary>
    /// 数据库类型<br/>
    /// </summary>
    [Display(Name = "Tenant.DbType")]
    public DbType? DbType { get; set; }

    /// <summary>
    /// 数据库连接
    /// </summary>
    [Display(Name = "Tenant.Connection")]
    public string ConnectionString { get; set; }
}

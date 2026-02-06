using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.ViewModel.Report.System;

/// <summary>
/// 租户导出模板
/// </summary>
public class TenantExport : ExportBase
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [Display(Name = "Tenant.Id")]
    public int TenantId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "Tenant.Name")]
    public string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "Sys.Description")]
    public string Description { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    [Display(Name = "Tenant.TenantType")]
    public TenantType TenantType { get; set; }

    /// <summary>
    /// 库Id
    /// </summary>
    [Display(Name = "Tenant.ConfigId")]
    public string ConfigId { get; set; }

    /// <summary>
    /// 库类型
    /// </summary>
    [Display(Name = "Tenant.DbType")]
    public DbType? DbType { get; set; }

    /// <summary>
    /// 库连接
    /// </summary>
    [Display(Name = "Tenant.ConnectionString")]
    public string ConnectionString { get; set; }
}

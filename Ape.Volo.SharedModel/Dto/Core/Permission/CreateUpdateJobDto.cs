using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.Permission;

namespace Ape.Volo.SharedModel.Dto.Core.Permission;

/// <summary>
/// 岗位Dto
/// </summary>
[AutoMapping(typeof(Job), typeof(CreateUpdateJobDto))]
public class CreateUpdateJobDto : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "Job.Name")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "Sys.Sort")]
    [Range(1, 999, ErrorMessage = "{0}range{1}{2}")]
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }
}

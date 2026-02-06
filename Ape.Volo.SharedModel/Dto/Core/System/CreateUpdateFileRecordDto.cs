using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.System;

namespace Ape.Volo.SharedModel.Dto.Core.System;

/// <summary>
/// 文件记录Dto
/// </summary>
[AutoMapping(typeof(FileRecord), typeof(CreateUpdateFileRecordDto))]
public class CreateUpdateFileRecordDto : BaseEntityDto<long>
{
    /// <summary>
    /// 描述 Sys.Description
    /// </summary>
    [Display(Name = "Sys.Description")]
    [Required(ErrorMessage = "{0}required")]
    public string Description { get; set; }
}

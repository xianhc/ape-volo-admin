using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.SystemManagement.FileRecords.Models;

/// <summary>
/// 文件记录Dto
/// </summary>
public class CreateUpdateFileRecordDto : BaseEntityDto<long>
{
    /// <summary>
    /// 描述 Sys.Description
    /// </summary>
    [Display(Name = "描述")]
    [Required(ErrorMessage = "{0}required")]
    public string Description { get; set; } = string.Empty;
}

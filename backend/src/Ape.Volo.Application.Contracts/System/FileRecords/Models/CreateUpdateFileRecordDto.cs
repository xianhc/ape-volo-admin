using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.System.FileRecords.Models;

/// <summary>
/// 文件记录Dto
/// </summary>
public sealed record CreateUpdateFileRecordDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 描述 Sys.Description
    /// </summary>
    [Display(Name = "描述")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Description { get; init; } = string.Empty;
}

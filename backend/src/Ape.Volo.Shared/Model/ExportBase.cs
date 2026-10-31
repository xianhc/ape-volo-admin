using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Shared.Model;

/// <summary>
/// 导出模型的公共字段基类。
/// </summary>
public class ExportBase
{
    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// ID
    /// </summary>
    [Display(Name = "ID")]
    public long Id { get; set; }
}

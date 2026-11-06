namespace Ape.Volo.Application.Contracts.System.Dictionaries.Models;

/// <summary>
/// 字典详情Vo
/// </summary>
public class DictDetailVo : BaseEntityDto<long>
{
    /// <summary>
    /// 字典ID
    /// </summary>
    //[JsonIgnore]
    //[JsonProperty]
    public long DictId { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    public int DictSort { get; set; }
}

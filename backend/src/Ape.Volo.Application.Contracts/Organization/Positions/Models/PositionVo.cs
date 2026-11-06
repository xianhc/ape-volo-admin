namespace Ape.Volo.Application.Contracts.Organization.Positions.Models;

/// <summary>
/// 岗位Vo
/// </summary>
public class PositionVo : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }
}

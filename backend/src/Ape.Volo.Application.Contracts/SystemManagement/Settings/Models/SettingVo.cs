namespace Ape.Volo.Application.Contracts.SystemManagement.Settings.Models;

/// <summary>
/// 系统配置Vo
/// </summary>
public class SettingVo : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}
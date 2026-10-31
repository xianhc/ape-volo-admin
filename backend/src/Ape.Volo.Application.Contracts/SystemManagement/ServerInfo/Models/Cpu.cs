

namespace Ape.Volo.Application.Contracts.SystemManagement.ServerInfo.Models;

/// <summary>
/// 
/// </summary>
public class Cpu
{
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Package { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Core { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public int CoreNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Logic { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Used { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Idle { get; set; } = string.Empty;
}

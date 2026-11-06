using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Identity.Roles.Models;

/// <summary>
/// 角色Vo
/// </summary>
public class RoleSmallVo
{
    /// <summary>
    /// ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 权限标识符
    /// </summary>
    public string? AuthCode { get; set; }

    /// <summary>
    /// 数据权限
    /// </summary>
    public DataScopeType DataScopeType { get; set; }
}

namespace Ape.Volo.Application.Contracts.Identity.Models;

/// <summary>
/// 用户数据权限快照；非全部权限且用户集合为空时不授予访问权。
/// </summary>
public sealed record DataScopeSnapshot
{
    /// <summary>是否拥有全部数据权限。</summary>
    public bool AllowAll { get; init; }

    /// <summary>可访问的创建者主键集合。</summary>
    public long[] UserIds { get; init; } = [];
}

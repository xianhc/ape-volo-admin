namespace Ape.Volo.Web.Authentication;

/// <summary>
/// 请求内的数据权限快照。加载完成前、失败或正常空范围均不授予受限数据访问权。
/// </summary>
/// <param name="user">当前请求身份</param>
public sealed class RequestDataScope(ICurrentUser user)
{
    private long[]? _userIds;

    /// <summary>
    /// 当前请求是否需要按用户限制数据范围。
    /// </summary>
    public bool IsRestricted => user.IsAuthenticated;
    /// <summary>
    /// 是否拥有全部数据权限。
    /// </summary>
    public bool AllowAll { get; private set; }
    /// <summary>
    /// 允许访问的创建者主键集合；未加载完成时为空。
    /// </summary>
    public long[] UserIds => _userIds ?? [];

    /// <summary>
    /// 异步获取并复制权限，避免与缓存中的可变集合共享状态。
    /// </summary>
    /// <param name="service">数据权限服务</param>
    /// <param name="ct">取消令牌</param>
    public async Task InitializeAsync(IDataScopeService service, CancellationToken ct)
    {
        AllowAll = await service.HasAllDataScopeAsync(user.Id, ct);
        ct.ThrowIfCancellationRequested();
        if (AllowAll)
        {
            return;
        }

        var userIds = await service.GetDataScopeUserIdsAsync(user.Id, ct);
        ct.ThrowIfCancellationRequested();
        _userIds = userIds.ToArray();
    }
}

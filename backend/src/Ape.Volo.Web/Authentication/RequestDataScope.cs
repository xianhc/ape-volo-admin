namespace Ape.Volo.Web.Authentication;

/// <summary>
/// 请求内的数据权限快照。加载完成前及失败时默认只允许当前用户自己的数据。
/// </summary>
/// <param name="user">当前请求身份</param>
public sealed class RequestDataScope(ICurrentUser user)
{
    private long[]? _userIds;

    /// <summary>
    /// 当前请求是否需要按用户限制数据范围。
    /// </summary>
    public bool IsRestricted => !string.IsNullOrEmpty(user.Account);
    /// <summary>
    /// 是否拥有全部数据权限。
    /// </summary>
    public bool AllowAll { get; private set; }
    /// <summary>
    /// 允许访问的创建者主键集合；未加载完成时收敛为当前用户。
    /// </summary>
    public long[] UserIds => _userIds ?? (user.Id > 0 ? [user.Id] : []);

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
        if (userIds.Count > 0) _userIds = userIds.ToArray();
    }
}

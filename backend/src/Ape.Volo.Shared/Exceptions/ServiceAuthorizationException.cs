namespace Ape.Volo.Shared.Exceptions;

/// <summary>
/// 应用服务授权失败；内部调用直接接收异常，HTTP 宿主根据身份映射为 401 或 403。
/// </summary>
/// <param name="isAuthenticated">当前调用者是否已登录</param>
public sealed class ServiceAuthorizationException(bool isAuthenticated)
    : Exception(isAuthenticated ? "抱歉，您没有执行此操作的权限。" : "请先登录后再执行此操作。")
{
    /// <summary>
    /// 已登录但无权限为 true，缺少有效身份为 false。
    /// </summary>
    public bool IsAuthenticated { get; } = isAuthenticated;
}

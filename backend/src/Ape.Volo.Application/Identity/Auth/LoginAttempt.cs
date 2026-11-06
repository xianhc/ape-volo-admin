namespace Ape.Volo.Application.Identity.Auth;

/// <summary>
/// 登录尝试：失败次数与锁定状态，随缓存一起序列化。
/// </summary>
internal sealed class LoginAttempt
{
    /// <summary>
    /// 连续失败次数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 是否已锁定
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// 解锁时间
    /// </summary>
    public DateTime LockUntil { get; set; }
}

namespace Ape.Volo.Application.Contracts.Abstractions;

/// <summary>
/// 业务所需的当前身份，不暴露 HTTP 请求或认证凭证。
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// 用户 ID；匿名或后台为 0。
    /// </summary>
    long Id { get; }
    /// <summary>
    /// 用户账号；无账号时为空字符串。
    /// </summary>
    string Account { get; }
    /// <summary>
    /// 所属部门 ID。
    /// </summary>
    long DeptId { get; }
    /// <summary>
    /// 是否已通过身份认证。
    /// </summary>
    bool IsAuthenticated { get; }
}

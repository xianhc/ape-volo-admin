using Ape.Volo.Application.Contracts.Identity.Users.Models;

namespace Ape.Volo.Application.Contracts.Identity.Users;

/// <summary>
/// 登录和身份解析使用的内部用户读取，不继承远程标记，也不承担用户管理操作。
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// 读取指定身份及其角色信息。
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户信息</returns>
    Task<UserVo> GetByIdAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 登录时按账号或邮箱读取用户。
    /// </summary>
    /// <param name="name">账号或邮箱</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户，不存在时为 null</returns>
    Task<UserVo?> GetByNameAsync(string name, CancellationToken ct = default);
}

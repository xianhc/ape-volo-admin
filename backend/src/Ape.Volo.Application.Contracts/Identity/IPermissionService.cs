namespace Ape.Volo.Application.Contracts.Identity;

/// <summary>
/// 权限信息接口
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 获取用户当前角色代码，供授权检查使用，不走用户管理应用方法。
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>角色代码集合</returns>
    Task<List<string>> GetRoleCodesAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 获取权限标识符
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<string>> GetAuthCodeAsync(long userId, CancellationToken ct = default);
}

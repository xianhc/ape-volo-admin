namespace Ape.Volo.Application.Contracts.Permission;

/// <summary>
/// 数据权限接口
/// </summary>
public interface IDataScopeService
{
    /// <summary>
    /// 判断指定用户是否拥有全部数据权限
    /// </summary>
    /// <param name="userId">用户主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>拥有全部数据权限时为 true</returns>
    Task<bool> HasAllDataScopeAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 获取指定用户在数据权限范围内可访问的用户主键集合
    /// </summary>
    /// <param name="userId">用户主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>可访问的用户主键集合，无任何范围时为空集合</returns>
    Task<List<long>> GetDataScopeUserIdsAsync(long userId, CancellationToken ct = default);
}

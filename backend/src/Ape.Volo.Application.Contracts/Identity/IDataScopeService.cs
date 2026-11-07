using Ape.Volo.Application.Contracts.Identity.Models;

namespace Ape.Volo.Application.Contracts.Identity;

/// <summary>
/// 数据权限接口
/// </summary>
public interface IDataScopeService
{
    /// <summary>
    /// 获取指定用户的数据权限快照
    /// </summary>
    /// <param name="userId">用户主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>全部权限标识及可访问的用户主键集合；空范围不授予访问权</returns>
    Task<DataScopeSnapshot> GetDataScopeAsync(long userId, CancellationToken ct = default);
}

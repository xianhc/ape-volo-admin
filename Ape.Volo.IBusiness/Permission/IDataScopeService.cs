using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ape.Volo.IBusiness.Permission;

/// <summary>
/// 数据权限接口
/// </summary>
public interface IDataScopeService
{
    /// <summary>
    /// 获取指定用户在数据权限范围内可访问的所有用户账号（用户名）
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<string>> GetDataScopeAccountsAsync(long userId);
}

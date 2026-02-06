using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Entity.Core.Permission.Role;
using Ape.Volo.ViewModel.Jwt;

namespace Ape.Volo.IBusiness.Permission;

/// <summary>
/// 权限信息接口
/// </summary>
public interface IPermissionService : IBaseServices<Role>
{
    /// <summary>
    /// 获取权限标识符
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <returns></returns>
    Task<List<string>> GetAuthCodeAsync(long userId);


    /// <summary>
    /// 获取权限urls
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    Task<List<UrlAccessControlVo>> GetUrlAccessControlAsync(long userId);
}

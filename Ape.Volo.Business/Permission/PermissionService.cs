using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.Permission;
using Ape.Volo.Entity.Core.Permission.Role;
using Ape.Volo.Entity.Core.Permission.User;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.ViewModel.Jwt;

namespace Ape.Volo.Business.Permission;

/// <summary>
/// 权限服务
/// </summary>
public class PermissionService : BaseServices<Role>, IPermissionService
{
    #region 基础方法

    /// <summary>
    /// 获取权限标识符
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <returns></returns>
    [UseCache(Expiration = 60, KeyPrefix = GlobalConstants.CachePrefix.UserAuthCodes)]
    public async Task<List<string>> GetAuthCodeAsync(long userId)
    {
        var authCodeList = await SugarClient
            .Queryable<UserRole, RoleMenu, Menu>((ur, rm, m) => ur.RoleId == rm.RoleId && rm.MenuId == m.Id)
            .GroupBy((ur, rm, m) => m.AuthCode)
            .Where((ur, rm, m) => ur.UserId == userId && m.MenuType != MenuType.Catalog && m.AuthCode != null)
            .OrderBy((ur, rm, m) => m.AuthCode)
            .ClearFilter<ICreateByEntity>()
            .Select((ur, rm, m) => m.AuthCode).ToListAsync();
        authCodeList = authCodeList.Where(x => !x.IsNullOrEmpty()).ToList();
        return authCodeList;
    }


    /// <summary>
    /// 获取权限urls
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [UseCache(Expiration = 60, KeyPrefix = GlobalConstants.CachePrefix.UserAuthUrls)]
    public async Task<List<UrlAccessControlVo>> GetUrlAccessControlAsync(long userId)
    {
        var urlAccessControlList = await SugarClient
            .Queryable<UserRole, RoleApis, Apis>((ur, ra, a) => ur.RoleId == ra.RoleId && ra.ApisId == a.Id)
            .GroupBy((ur, ra, a) => new { a.Url, a.Method })
            .Where(ur => ur.UserId == userId)
            .OrderBy((ur, ra, a) => a.Url)
            .ClearFilter<ICreateByEntity>()
            .Select((ur, ra, a) => new UrlAccessControlVo
            {
                Url = a.Url,
                Method = a.Method
            }).ToListAsync();
        urlAccessControlList = urlAccessControlList.Where(x => !x.IsNullOrEmpty()).ToList();
        return urlAccessControlList;
    }

    #endregion
}

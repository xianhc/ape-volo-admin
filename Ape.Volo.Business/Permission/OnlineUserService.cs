using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Common.WebApp;
using Ape.Volo.Core.Caches;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.ViewModel.Core.Permission.User;
using Ape.Volo.ViewModel.Jwt;
using Ape.Volo.ViewModel.Report.Monitor;
using IP2Region.Net.Abstractions;
using Shyjus.BrowserDetection;
using IOnlineUserService = Ape.Volo.IBusiness.Permission.IOnlineUserService;

namespace Ape.Volo.Business.Permission;

/// <summary>
/// 在线用户service
/// </summary>
public class OnlineUserService : IOnlineUserService
{
    #region 字段

    private readonly IBrowserDetector _browserDetector;
    private readonly ISearcher _ipSearcher;
    private readonly ICache _cache;
    private readonly ITokenBlacklistService _tokenBlacklistService;

    #endregion

    #region 构造函数

    /// <summary>
    /// 
    /// </summary>
    /// <param name="browserDetector"></param>
    /// <param name="searcher"></param>
    /// <param name="cache"></param>
    /// <param name="tokenBlacklistService"></param>
    public OnlineUserService(IBrowserDetector browserDetector, ISearcher searcher, ICache cache,
        ITokenBlacklistService tokenBlacklistService)
    {
        _browserDetector = browserDetector;
        _ipSearcher = searcher;
        _cache = cache;
        _tokenBlacklistService = tokenBlacklistService;
    }

    #endregion

    #region 基础方法

    /// <summary>
    /// 保存在线用户
    /// </summary>
    /// <param name="userVo"></param>
    /// <param name="remoteIp"></param>
    public async Task<LoginUserInfo> SaveLoginUserAsync(UserVo userVo, string remoteIp)
    {
        var onlineUser = new LoginUserInfo
        {
            UserId = userVo.Id,
            Account = userVo.UserName,
            NickName = userVo.NickName,
            DeptId = userVo.DeptId,
            DeptName = userVo.Dept.Name,
            Ip = remoteIp,
            Address = _ipSearcher.Search(remoteIp),
            OperatingSystem = _browserDetector.Browser?.OS,
            DeviceType = _browserDetector.Browser?.DeviceType,
            BrowserName = _browserDetector.Browser?.Name,
            Version = _browserDetector.Browser?.Version,
            LoginTime = DateTime.Now,
            TenantId = userVo.TenantId
        };
        return await Task.FromResult(onlineUser);
    }

    /// <summary>
    /// 创建Jwt对象
    /// </summary>
    /// <param name="userVo"></param>
    /// <param name="authCodes"></param>
    /// <returns></returns>
    public async Task<JwtUserVo> CreateJwtUserAsync(UserVo userVo, List<string> authCodes)
    {
        var jwtUser = new JwtUserVo
        {
            User = userVo, RoleCodes = userVo.Roles.Select(x => x.AuthCode).ToList(), AuthCodes = authCodes
        };
        return await Task.FromResult(jwtUser);
    }


    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<LoginUserInfo>> QueryAsync(Pagination pagination)
    {
        List<LoginUserInfo> loginUserInfos = new List<LoginUserInfo>();
        var arrayList = await _cache.ScriptEvaluateKeys(GlobalConstants.CachePrefix.OnlineKey);
        if (arrayList.Length > 0)
        {
            foreach (var item in arrayList)
            {
                var loginUserInfo =
                    await _cache.GetAsync<LoginUserInfo>(item);
                if (loginUserInfo.IsNull()) continue;
                loginUserInfo.AccessToken = loginUserInfo.AccessToken.ToMd5String16();
                loginUserInfos.Add(loginUserInfo);
            }
        }

        List<LoginUserInfo> newOnlineUsers = new List<LoginUserInfo>();
        if (loginUserInfos.Count > 0)
        {
            newOnlineUsers = loginUserInfos.Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();
        }

        return newOnlineUsers;
    }

    /// <summary>
    /// 强退
    /// </summary>
    /// <param name="ids"></param>
    public async Task DropOutAsync(HashSet<string> ids)
    {
        var list = new List<TokenBlacklist>();
        list.AddRange(ids.Select(x => new TokenBlacklist { AccessToken = x }));
        if (await _tokenBlacklistService.AddAsync(list))
        {
            foreach (var item in ids)
            {
                await _cache.RemoveAsync(GlobalConstants.CachePrefix.OnlineKey + item);
            }
        }
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync()
    {
        List<ExportBase> onlineUserExports = new List<ExportBase>();
        var arrayList = await _cache.ScriptEvaluateKeys(GlobalConstants.CachePrefix.OnlineKey);
        if (arrayList.Length > 0)
        {
            foreach (var item in arrayList)
            {
                LoginUserInfo loginUserInfo =
                    await _cache.GetAsync<LoginUserInfo>(item);
                if (loginUserInfo != null)
                {
                    onlineUserExports.Add(loginUserInfo.ChangeType<OnlineUserExport>());
                }
            }
        }

        return onlineUserExports;
    }

    #endregion
}
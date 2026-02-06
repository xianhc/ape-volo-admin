using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Common.WebApp;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.ViewModel.Core.Permission.User;
using Ape.Volo.ViewModel.Jwt;

namespace Ape.Volo.IBusiness.Permission;

/// <summary>
/// 在线用户接口
/// </summary>
public interface IOnlineUserService
{
    #region 基础接口

    /// <summary>
    /// 保存在线用户
    /// </summary>
    /// <param name="userVo"></param>
    /// <param name="remoteIp"></param>
    Task<LoginUserInfo> SaveLoginUserAsync(UserVo userVo, string remoteIp);

    /// <summary>
    /// jwt用户信息
    /// </summary>
    /// <param name="userVo"></param>
    /// <param name="authCodeList"></param>
    /// <returns></returns>
    Task<JwtUserVo> CreateJwtUserAsync(UserVo userVo, List<string> authCodeList);


    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<LoginUserInfo>> QueryAsync(Pagination pagination);

    /// <summary>
    /// 强退
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task DropOutAsync(HashSet<string> ids);

    /// <summary>
    /// 下载
    /// </summary>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync();

    #endregion
}
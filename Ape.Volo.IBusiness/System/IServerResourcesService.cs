using System.Threading.Tasks;
using Ape.Volo.ViewModel.ServerInfo;

namespace Ape.Volo.IBusiness.System;

/// <summary>
/// 服务器资源信息接口
/// </summary>
public interface IServerResourcesService
{
    /// <summary>
    /// 查询
    /// </summary>
    /// <returns></returns>
    Task<ServerResourcesInfo> Query();
}
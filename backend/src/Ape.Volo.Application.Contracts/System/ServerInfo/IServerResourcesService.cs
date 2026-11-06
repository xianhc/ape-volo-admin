

using Ape.Volo.Application.Contracts.System.ServerInfo.Models;

namespace Ape.Volo.Application.Contracts.System.ServerInfo;

/// <summary>
/// 服务器资源信息接口
/// </summary>
public interface IServerResourcesService : IApplicationService
{
    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<ServerResourcesInfo> GetList(CancellationToken ct = default);
}

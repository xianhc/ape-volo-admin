using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.IBusiness.System;
using Ape.Volo.ViewModel.ServerInfo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.System;

/// <summary>
/// 服务器管理
/// </summary>
[Area("Area.ServerResourceManagement")]
[Route("/service", Order = 16)]
public class ServerResourcesController : BaseApiController
{
    private readonly IServerResourcesService _serverResourcesService;

    public ServerResourcesController(IServerResourcesService serverResourcesService)
    {
        _serverResourcesService = serverResourcesService;
    }

    #region 对内接口

    [HttpGet]
    [Route("resources/info")]
    [Description("Action.ServerResourceInfo")]
    [NotOperate]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServerResourcesInfo))]
    public async Task<ActionResult> Query()
    {
        if (!App.GetOptions<SystemOptions>().UseRedisCache)
        {
            return Error("该功能需要使用Redis缓存，请配置UseServerResources为True使用。");
        }

        var resourcesInfo = await _serverResourcesService.Query();

        return JsonContent(resourcesInfo);
    }

    #endregion
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Entity.Core.Permission;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Ape.Volo.Api.Controllers.Permission;

/// <summary>
/// Api管理
/// </summary>
[Area("Area.ApiManagement")]
[Route("/apis", Order = 20)]
public class ApisController : BaseApiController
{
    #region 字段

    private readonly IApisService _apisService;

    #endregion

    #region 构造函数

    public ApisController(IApisService apisService)
    {
        _apisService = apisService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增Api
    /// </summary>
    /// <param name="createUpdateApisDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create(
        [FromBody] CreateUpdateApisDto createUpdateApisDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _apisService.CreateAsync(createUpdateApisDto);
        return Ok(result);
    }

    /// <summary>
    /// 更新Api
    /// </summary>
    /// <param name="createUpdateApisDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(
        [FromBody] CreateUpdateApisDto createUpdateApisDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _apisService.UpdateAsync(createUpdateApisDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除Api
    /// </summary>
    /// <param name="idCollection"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("delete")]
    [Description("Sys.Delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Delete([FromBody] IdCollection idCollection)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _apisService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 查看Apis列表
    /// </summary>
    /// <param name="apisQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<ApisVo>>))]
    public async Task<ActionResult> Query(ApisQueryCriteria apisQueryCriteria, Pagination pagination)
    {
        var apisList = await _apisService.QueryAsync(apisQueryCriteria, pagination);

        return JsonContent(apisList, pagination);
    }


    /// <summary>
    /// 获取API分组
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("group")]
    [Description("Action.GetApiGroup")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [HasRole(["admin"])]
    public Task<ActionResult> GetApiGroup()
    {
        List<string> apiGroups = new List<string>();
        var types = GlobalType.ApiTypes.Where(x =>
                x.IsClass && typeof(Controller).IsAssignableFrom(x) && x.Name != "TestController" &&
                x.Namespace != "Ape.Volo.Api.Controllers.Base")
            .OrderBy(x => x.GetCustomAttributes<RouteAttribute>().FirstOrDefault()?.Order).ToList();
        foreach (var type in types)
        {
            var areaAttr = type.GetCustomAttributes(typeof(AreaAttribute), true)
                .OfType<AreaAttribute>()
                .FirstOrDefault();
            if (areaAttr != null)
            {
                if (!apiGroups.Contains(areaAttr.RouteValue))
                {
                    apiGroups.Add(App.L.R(areaAttr.RouteValue));
                }
            }
            else
            {
                if (!apiGroups.Contains(type.Name))
                {
                    apiGroups.Add(type.Name);
                }
            }
        }

        return Task.FromResult<ActionResult>(JsonContent(apiGroups));
    }


    /// <summary>
    /// 刷新Api列表 只实现了api新增添加
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("refresh")]
    [Description("Action.RefreshApi")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> RefreshApis()
    {
        List<Apis> apis = new List<Apis>();
        var allApis = await _apisService.QueryAllAsync();
        var types = GlobalType.ApiTypes.Where(x =>
                x.IsClass && typeof(Controller).IsAssignableFrom(x) && x.Name != "TestController" &&
                x.Name != "HealthCheckController" &&
                x.Namespace != "Ape.Volo.Api.Controllers.Base")
            .OrderBy(x => x.GetCustomAttributes<RouteAttribute>().FirstOrDefault()?.Order).ToList();
        foreach (var type in types)
        {
            var areaAttr = type.GetCustomAttributes(typeof(AreaAttribute), true)
                .OfType<AreaAttribute>()
                .FirstOrDefault();
            var routeAttr = type.GetCustomAttributes(typeof(RouteAttribute), true)
                .OfType<RouteAttribute>()
                .FirstOrDefault();
            var methods = type.GetMethods().Where(m =>
                m.DeclaringType == type && !Attribute.IsDefined(m, typeof(NonActionAttribute)));

            foreach (var methodInfo in methods)
            {
                var methodAttr = methodInfo.GetCustomAttributes(typeof(HttpMethodAttribute), true)
                    .OfType<HttpMethodAttribute>()
                    .FirstOrDefault();
                var methodRouteAttr = methodInfo.GetCustomAttributes(typeof(RouteAttribute), true)
                    .OfType<RouteAttribute>()
                    .FirstOrDefault();
                var url = $"{routeAttr?.Template}/{methodRouteAttr?.Template}".ToLower();
                var method = methodAttr?.HttpMethods.FirstOrDefault()?.Trim();
                if (!allApis.Any(x =>
                        x.Url.Equals(url, StringComparison.CurrentCultureIgnoreCase) && x.Method == method))
                {
                    if (method != null)
                    {
                        var group = areaAttr != null ? App.L.R(areaAttr.RouteValue) : type.Name;
                        var descriptionAttr = methodInfo.GetCustomAttributes(typeof(DescriptionAttribute), true)
                            .OfType<DescriptionAttribute>()
                            .FirstOrDefault();
                        var description = descriptionAttr != null
                            ? App.L.R(descriptionAttr.Description)
                            : methodInfo.Name;
                        apis.Add(new Apis
                        {
                            Id = IdHelper.NextId(),
                            Group = group,
                            Url = url,
                            Description = description,
                            Method = method
                        });
                    }
                }
            }
        }

        if (apis.Count == 0)
        {
            return Ok(OperateResult.Success(App.L.R("Action.ApiRefresh.Success")));
        }

        var result = await _apisService.CreateAsync(apis);
        if (result.IsSuccess)
        {
            return Ok(OperateResult.Success(App.L.R("Action.ApiRefresh.Success")));
        }

        return Ok(OperateResult.Success(App.L.R("Action.ApiRefresh.Fail")));
    }

    /// <summary>
    /// 查询所有Api
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("queryAll")]
    [Description("Action.GetAllApi")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ApisVo>))]
    public async Task<ActionResult> QueryAllApis()
    {
        List<ApisTree> apisTree = new List<ApisTree>();
        var apis = await _apisService.QueryAllAsync();
        var apisGroup = apis.GroupBy(x => x.Group).ToList();

        var index = 0;
        foreach (var g in apisGroup)
        {
            var apisTreesTmp = new List<ApisTree>();
            foreach (var api in g.ToList())
            {
                apisTreesTmp.Add(new ApisTree
                {
                    Id = api.Id,
                    Label = api.Description + " => " + api.Url + "",
                    Leaf = true,
                    HasChildren = false,
                    Children = null
                });
            }

            index++;
            apisTree.Add(new ApisTree
            {
                Id = index,
                Label = g.Key,
                Leaf = false,
                HasChildren = true,
                Children = apisTreesTmp
            });
        }

        return JsonContent(apisTree);
    }

    #endregion
}
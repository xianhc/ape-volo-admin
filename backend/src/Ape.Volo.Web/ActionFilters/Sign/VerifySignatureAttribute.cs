using System.Security.Cryptography;
using System.Text;
using Ape.Volo.Application.Contracts.SystemManagement.AppSecrets.Models;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Caches;
using Ape.Volo.Core.Utils;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Web.ActionFilters.Sign;

/// <summary>
/// 接口对外签名验证
/// </summary>
public class VerifySignatureAttribute : BaseActionFilter
{
    /// <summary>
    /// Action执行之前执行
    /// </summary>
    /// <param name="filterContext"></param>
    public override async Task OnActionExecuting(ActionExecutingContext filterContext)
    {
        //判断是否需要签名
        if (filterContext.ContainsFilter<IgnoreVerifySignatureAttribute>())
            return;
        var request = filterContext.HttpContext.Request;

        var appId = request.Headers["appId"].ToString();
        if (appId.IsNullOrEmpty())
        {
            filterContext.Result = Error(filterContext.HttpContext, "appId 是必填项");
            return;
        }

        var time = request.Headers["time"].ToString();
        if (time.IsNullOrEmpty())
        {
            filterContext.Result = Error(filterContext.HttpContext, "time 是必填项");
            return;
        }

        // 头内容由调用方控制，解析失败按参数错误处理，不能落到 500 分支
        if (!DateTime.TryParse(time, out var requestTime))
        {
            filterContext.Result = Error(filterContext.HttpContext, "time 格式错误");
            return;
        }

        if (requestTime < DateTime.Now.AddMinutes(-5) || requestTime > DateTime.Now.AddMinutes(5))
        {
            filterContext.Result = Error(filterContext.HttpContext, "time过期");
            return;
        }

        var guid = request.Headers["guid"].ToString();
        if (guid.IsNullOrEmpty())
        {
            filterContext.Result = Error(filterContext.HttpContext, "guid 是必填项");
            return;
        }

        var guidKey = $"ApiGuid_{appId}_{guid}";
        var cache = filterContext.HttpContext.RequestServices.GetRequiredService<ICache>();
        if (!(await cache.GetAsync<string>(guidKey)).IsNullOrEmpty())
        {
            filterContext.Result = Error(filterContext.HttpContext, "禁止重复调用");
            return;
        }

        request.EnableBuffering();
        var body = await Requests.RequestParameterReader.ReadBodyAsync(request, filterContext.HttpContext.RequestAborted);

        var sign = request.Headers["sign"].ToString();
        if (sign.IsNullOrEmpty())
        {
            filterContext.Result = Error(filterContext.HttpContext, "sign 是必填项");
            return;
        }

        var appSecretModel = await filterContext.HttpContext.RequestServices.GetRequiredService<IAppSecretService>()
            .QueryByAppIdAsync(appId);
        if (appSecretModel == null)
        {
            CreateUpdateAppSecretDto createUpdateAppSecretDto = new CreateUpdateAppSecretDto
            {
                AppId = appId
            };
            filterContext.Result =
                Error(filterContext.HttpContext, ValidationError.NotExist(createUpdateAppSecretDto, "App应用密钥",
                    nameof(createUpdateAppSecretDto.AppId)));
            return;
        }

        var newSign = BuildApiSign(appId, appSecretModel.AppSecretKey, guid, requestTime, body);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(sign.ToLowerInvariant()),
                Encoding.UTF8.GetBytes(newSign)))
        {
            filterContext.Result = Error(filterContext.HttpContext, "签名验证失败");
            return;
        }

        var replayMarkerStored = await cache.SetAsync(guidKey, "1", TimeSpan.FromMinutes(10),
            CacheExpireType.Absolute);
        if (!replayMarkerStored)
        {
            filterContext.Result = Error(filterContext.HttpContext, "签名状态保存失败，请稍后重试");
        }
    }

    /// <summary>
    /// 生成接口签名sign
    /// 注：md5(appId+time+guid+body+appSecret)
    /// </summary>
    /// <param name="appId">应用Id</param>
    /// <param name="appSecret">应用密钥</param>
    /// <param name="guid">唯一GUID</param>
    /// <param name="time">时间</param>
    /// <param name="body">请求体</param>
    /// <returns></returns>
    private string BuildApiSign(string appId, string appSecret, string guid, DateTime time, string body)
    {
        return $"{appId}{time:yyyy-MM-dd HH:mm:ss}{guid}{body}{appSecret}".ToMd5String();
    }
}

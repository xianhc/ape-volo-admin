using Microsoft.AspNetCore.Authorization;

namespace Ape.Volo.Web.Controllers.Base;

/// <summary>
/// 接口控制器基类，统一要求身份认证；业务权限由应用服务代理检查。
/// </summary>
// 公开接口必须显式标记 [AllowAnonymous]。
[Authorize]
public class BaseApiController : BaseController
{
}

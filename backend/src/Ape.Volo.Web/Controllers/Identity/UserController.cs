using System.ComponentModel;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Web.Controllers.Identity;

/// <summary>
/// 用户头像上传适配，其余用户动作由动态 API 提供。
/// </summary>
[Area("用户管理")]
[Route("/api/user")]
public class UserController(IUserService users) : BaseApiController
{
    /// <summary>
    /// 修改当前登录用户的头像，用户身份由应用服务中的 ICurrentUser 决定。
    /// </summary>
    /// <param name="file">头像文件</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新结果</returns>
    [HttpPost("update-avatar")]
    [Description("修改个人头像")]
    public async Task<ActionResult> UpdateAvatar([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null)
            return Error("file 是必填项");

        await using var uploadFile = file.ToUploadFile();
        return Ok(await users.UpdateAvatarAsync(uploadFile, ct));
    }
}

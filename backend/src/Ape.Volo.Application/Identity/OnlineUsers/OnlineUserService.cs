using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Identity.OnlineUsers.Models;
using Ape.Volo.Shared.Exceptions;
using Ape.Volo.Application.Contracts.Identity.Users.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Shared.WebApp;
using Newtonsoft.Json;

namespace Ape.Volo.Application.Identity.OnlineUsers;

/// <summary>
/// 在线用户列表、导出及强退仅对 Redis 缓存开放。
/// </summary>
public class OnlineUserService(
    IClientEnvironment clientEnvironment,
    IAuthSessionService sessions, IEnumerable<ICacheKeyCatalog> keyCatalogs) : IOnlineUserService
{
    /// <inheritdoc />
    public Task<LoginUserInfo> SaveLoginUserAsync(UserVo userVo, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(new LoginUserInfo
        {
            UserId = userVo.Id,
            Account = userVo.UserName,
            NickName = userVo.NickName ?? string.Empty,
            DeptId = userVo.DeptId,
            DeptName = userVo.Dept?.Name ?? string.Empty,
            Ip = clientEnvironment.IpAddress,
            Address = clientEnvironment.Location,
            OperatingSystem = clientEnvironment.OperatingSystem,
            DeviceType = clientEnvironment.DeviceType,
            BrowserName = clientEnvironment.BrowserName,
            Version = clientEnvironment.BrowserVersion,
            LoginTime = DateTime.Now
        });
    }

    /// <inheritdoc />
    public Task<JwtUserVo> CreateJwtUserAsync(UserVo userVo, List<string> authCodes, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(new JwtUserVo
        {
            User = userVo,
            RoleCodes = userVo.Roles.Select(x => x.AuthCode ?? string.Empty).ToList(),
            AuthCodes = authCodes
        });
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.OnlineUsers.List)]
    public async Task<PagedResultDto<LoginUserInfo>> GetListAsync(PagedAndSortedResultRequestDto request,
        CancellationToken ct = default)
    {
        var pagination = Pagination.From(request);
        var active = await sessions.ListActiveAsync(ct);
        var users = active.Select(x => JsonConvert.DeserializeObject<LoginUserInfo>(x.LoginInfoJson))
            .OfType<LoginUserInfo>().ToList();
        var items = users.Skip((pagination.PageIndex - 1) * pagination.PageSize).Take(pagination.PageSize).ToList();
        return new PagedResultDto<LoginUserInfo>(users.Count, items);
    }

    /// <inheritdoc />
    [HasRole(RoleCodes.Admin)]
    public async Task DropOutAsync(HashSet<string> ids, CancellationToken ct = default)
    {
        if (!keyCatalogs.Any())
            throw new BadRequestException("在线用户功能仅支持 Redis，请启用 UseRedisCache。");

        foreach (var id in ids)
            await sessions.RevokeAsync(id, ct);
    }

    /// <inheritdoc />
    [HasRole(RoleCodes.Admin)]
    public async Task<List<ExportBase>> DownloadAsync(CancellationToken ct = default)
    {
        var active = await sessions.ListActiveAsync(ct);
        return active.Select(x => JsonConvert.DeserializeObject<LoginUserInfo>(x.LoginInfoJson))
            .OfType<LoginUserInfo>().Select(x => x.ChangeType<OnlineUserExport>()).OfType<ExportBase>().ToList();
    }
}

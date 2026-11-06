using Microsoft.AspNetCore.Authorization;
using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Abstractions.Storage;
using Ape.Volo.Application.Contracts.Identity.Users.Models;
using Ape.Volo.Application.Contracts.Identity.Models;
using Ape.Volo.Application.System.FileRecords;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Organization.Positions;
using Ape.Volo.Entity.Identity.Users;
using Ape.Volo.Shared.Model;
using SqlSugar;

namespace Ape.Volo.Application.Identity.Users;

/// <summary>
/// 用户服务
/// </summary>
public class UserService : BaseService<User>, IUserService, IUserLookupService
{
    #region 字段

    private readonly IAuthSessionService _sessions;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUser _currentUser;
    private readonly ICache _cache;
    private readonly SystemOptions _systemOptions;
    private readonly IPasswordDecryptor _passwordDecryptor;
    private readonly IPermissionCacheService _permissionCache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationAuthorizationService _authorization;
    private readonly ISugarQueryProvider _sugarQueries;

    #endregion

    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="sessions">登录会话</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="fileStorage">上传文件存储路径</param>
    /// <param name="currentUser">当前请求用户</param>
    /// <param name="cache">缓存</param>
    /// <param name="systemOptions">系统配置</param>
    /// <param name="passwordDecryptor">密码传输解密器</param>
    /// <param name="permissionCache">用户权限缓存入口</param>
    /// <param name="unitOfWork">提交后动作登记</param>
    /// <param name="authorization">角色分配授权</param>
    /// <param name="sugarQueries">内部身份读取</param>
    public UserService(ISugarRepository<User> sugarRepository,
        IAuthSessionService sessions, IMapper mapper, IFileStorage fileStorage,
        ICurrentUser currentUser, ICache cache, IOptions<SystemOptions> systemOptions,
        IPasswordDecryptor passwordDecryptor, IPermissionCacheService permissionCache, IUnitOfWork unitOfWork,
        IApplicationAuthorizationService authorization, ISugarQueryProvider sugarQueries)
        : base(sugarRepository, mapper)
    {
        _sessions = sessions;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
        _cache = cache;
        _systemOptions = systemOptions.Value;
        _passwordDecryptor = passwordDecryptor;
        _permissionCache = permissionCache;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _sugarQueries = sugarQueries;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Users.Add)]
    public async Task<OperateResult> CreateAsync(CreateUpdateUserDto createUpdateUserDto, CancellationToken ct = default)
    {
        // 新用户包含角色分配，与专门的角色授权入口保持相同限制。
        await _authorization.CheckAsync([RoleCodes.Admin], [], ct);

        if (await TableWhere(x => x.UserName == createUpdateUserDto.UserName).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateUserDto.UserName);
        }

        if (await TableWhere(x => x.Email == createUpdateUserDto.Email).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateUserDto.Email);
        }

        if (!string.IsNullOrEmpty(createUpdateUserDto.Phone) &&
            await TableWhere(x => x.Phone == createUpdateUserDto.Phone).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateUserDto.Phone);
        }

        var user = Mapper.MapTo<User>(createUpdateUserDto);

        //设置用户密码
        user.Password = BCryptHelper.Hash(_systemOptions.UserDefaultPassword);
        user.DeptId = user.Dept!.Id;
        //用户
        await AddAsync(user, ct: ct);


        await SugarClient.Deleteable<UserRole>().Where(x => x.UserId == user.Id).ExecuteCommandAsync(ct);
        var userRoles = new List<UserRole>();
        userRoles.AddRange((user.Roles ?? []).Select(x => new UserRole { UserId = user.Id, RoleId = x.Id }));
        await SugarClient.Insertable(userRoles).ExecuteCommandAsync(ct);


        await SugarClient.Deleteable<UserPosition>().Where(x => x.UserId == user.Id).ExecuteCommandAsync(ct);
        var userPositions = new List<UserPosition>();
        userPositions.AddRange((user.Positions ?? []).Select(x => new UserPosition { UserId = user.Id, PositionId = x.Id }));
        await SugarClient.Insertable(userPositions).ExecuteCommandAsync(ct);

        await _permissionCache.UserChangedAsync(user.Id, CancellationToken.None);
        return OperateResult.Success();
    }

    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Users.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdateUserDto createUpdateUserDto, CancellationToken ct = default)
    {
        //取出待更新数据
        var oldUser = await TableWhere(x => x.Id == createUpdateUserDto.Id).Includes(x => x.Roles).FirstAsync(ct);
        if (oldUser.IsNull())
        {
            return NotFound(createUpdateUserDto.Id);
        }

        var requestedRoles = createUpdateUserDto.Roles.Select(role => role.Id).ToHashSet();
        var rolesChanged = !(oldUser.Roles ?? []).Select(role => role.Id).ToHashSet().SetEquals(requestedRoles);
        if (rolesChanged || oldUser.DeptId != createUpdateUserDto.Dept.Id)
            await _authorization.CheckAsync([RoleCodes.Admin], [], ct);

        if (oldUser.UserName != createUpdateUserDto.UserName &&
            await TableWhere(x => x.UserName == createUpdateUserDto.UserName).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateUserDto.UserName);
        }

        if (oldUser.Email != createUpdateUserDto.Email &&
            await TableWhere(x => x.Email == createUpdateUserDto.Email).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateUserDto.Email);
        }

        if (!string.IsNullOrEmpty(createUpdateUserDto.Phone) && oldUser.Phone != createUpdateUserDto.Phone &&
            await TableWhere(x => x.Phone == createUpdateUserDto.Phone).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateUserDto.Phone);
        }

        var user = Mapper.MapTo<User>(createUpdateUserDto);
        user.DeptId = user.Dept!.Id;
        //更新用户
        await UpdateAsync(user, null, x => new { x.Password, x.AvatarPath, x.PasswordReSetTime }, false, ct: ct);


        if (rolesChanged)
        {
            await SugarClient.Deleteable<UserRole>().Where(x => x.UserId == user.Id).ExecuteCommandAsync(ct);
            var userRoles = requestedRoles.Select(id => new UserRole { UserId = user.Id, RoleId = id }).ToList();
            await SugarClient.Insertable(userRoles).ExecuteCommandAsync(ct);
        }

        await SugarClient.Deleteable<UserPosition>().Where(x => x.UserId == user.Id).ExecuteCommandAsync(ct);
        var userPositions = new List<UserPosition>();
        userPositions.AddRange((user.Positions ?? []).Select(x => new UserPosition { UserId = user.Id, PositionId = x.Id }));
        await SugarClient.Insertable(userPositions).ExecuteCommandAsync(ct);

        //清理缓存
        await ClearUserCache(user.Id);
        await _unitOfWork.AfterCommitAsync(() => _sessions.RevokeUserAsync(user.Id, CancellationToken.None));
        return OperateResult.Success();
    }

    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Users.Delete)]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        if (ids.Contains(_currentUser.Id))
        {
            return OperateResult.Error("禁止删除自己");
        }

        var users = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        if (result)
            foreach (var user in users)
            {
                await ClearUserCache(user.Id);
                var userId = user.Id;
                await _unitOfWork.AfterCommitAsync(() => _sessions.RevokeUserAsync(userId, CancellationToken.None));
            }
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Users.List)]
    public async Task<PagedResultDto<UserVo>> GetListAsync(UserQueryCriteria userQueryCriteria, CancellationToken ct = default)
        => await PageAsync<UserVo>(userQueryCriteria, await GetConditionalModel(userQueryCriteria),
            options => options.IncludePropertyNameList = [nameof(User.Dept), nameof(User.Roles), nameof(User.Positions)], ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Users.Download)]
    public async Task<List<ExportBase>> DownloadAsync(UserQueryCriteria userQueryCriteria, CancellationToken ct = default)
    {
        var conditionalModels = await GetConditionalModel(userQueryCriteria);
        var users = await Table.Includes(x => x.Dept).Includes(x => x.Roles)
            .Includes(x => x.Positions).Where(conditionalModels).ToListAsync(ct);
        List<ExportBase> userExports = new List<ExportBase>();
        userExports.AddRange(users.Select(x => new UserExport
        {
            Id = x.Id,
            Username = x.UserName,
            Role = string.Join(",", (x.Roles ?? []).Select(r => r.Name).ToArray()),
            NickName = x.NickName ?? string.Empty,
            Phone = x.Phone ?? string.Empty,
            Email = x.Email ?? string.Empty,
            Enabled = x.Enabled,
            Dept = x.Dept?.Name ?? string.Empty,
            Position = string.Join(",", (x.Positions ?? []).Select(j => j.Name).ToArray()),
            GenderCode = x.GenderCode,
            CreationTime = x.CreationTime
        }));
        return userExports;
    }

    #endregion

    #region 扩展方法

    // 内部读取不继承用户管理入口的授权，资料缓存统一走权限缓存入口。
    /// <inheritdoc />
    Task<UserVo> IUserLookupService.GetByIdAsync(long userId, CancellationToken ct) => LoadUserByIdAsync(userId, ct);

    /// <inheritdoc />
    Task<UserVo?> IUserLookupService.GetByNameAsync(string name, CancellationToken ct) => LoadUserByNameAsync(name, ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Users.List)]
    public Task<UserVo> GetByIdAsync(long userId, CancellationToken ct = default) => LoadUserByIdAsync(userId, ct);

    private async Task<UserVo> LoadUserByIdAsync(long userId, CancellationToken ct)
    {
        return await _permissionCache.GetOrCreateAsync(PermissionCacheItem.UserInfo, userId, async () =>
        {
            var user = await _sugarQueries.QueryWithoutDataScopeAsync(db => db.Queryable<User>()
                .Where(x => x.Id == userId).Includes(x => x.Dept)
                .Includes(x => x.Roles).Includes(x => x.Positions).FirstAsync(ct), ct);
            return Mapper.MapTo<UserVo>(user);
        }, ct);
    }

    /// <inheritdoc />
    public Task<UserVo?> GetByNameAsync(string userName, CancellationToken ct = default) => LoadUserByNameAsync(userName, ct);

    private async Task<UserVo?> LoadUserByNameAsync(string userName, CancellationToken ct)
    {
        var isEmail = userName.IsEmail();
        var user = await _sugarQueries.QueryWithoutDataScopeAsync(db => isEmail
            ? db.Queryable<User>().Where(s => s.Email == userName).FirstAsync(ct)
            : db.Queryable<User>().Where(s => s.UserName == userName).FirstAsync(ct), ct);

        return user == null ? null : Mapper.MapTo<UserVo>(user);
    }

    /// <inheritdoc />
    [Authorize]
    public async Task<OperateResult> UpdateCenterAsync(UpdateUserCenterDto updateUserCenterDto, CancellationToken ct = default)
    {
        var user = await TableWhere(x => x.Id == _currentUser.Id).FirstAsync(ct);
        if (user.IsNull())
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(updateUserCenterDto.Phone) &&
            await TableWhere(x => x.Phone == updateUserCenterDto.Phone && x.Id != _currentUser.Id).AnyAsync(ct))
        {
            return AlreadyExists(() => updateUserCenterDto.Phone);
        }

        user.NickName = updateUserCenterDto.NickName;
        user.GenderCode = updateUserCenterDto.GenderCode;
        user.Phone = updateUserCenterDto.Phone;
        var result = await UpdateAsync(user, x => new { x.NickName, x.GenderCode, x.Phone }, isIgnoreNull: false, ct: ct);
        if (result) await ClearUserCache(user.Id);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [Authorize]
    public async Task<OperateResult> UpdatePasswordAsync(UpdateUserPassDto userPassDto, CancellationToken ct = default)
    {
        // 密文由客户端提供，格式非法时按输入错误返回，不抛服务端异常
        if (!_passwordDecryptor.TryDecrypt(userPassDto.OldPassword, out var oldPassword) ||
            !_passwordDecryptor.TryDecrypt(userPassDto.NewPassword, out var newPassword) ||
            !_passwordDecryptor.TryDecrypt(userPassDto.ConfirmPassword, out var confirmPassword))
        {
            return OperateResult.Error("密码格式错误");
        }

        if (oldPassword == newPassword)
            return OperateResult.Error("新密码不能与旧密码相同");

        if (!newPassword.Equals(confirmPassword))
        {
            return OperateResult.Error("两次输入不匹配");
        }

        var curUser = await TableWhere(x => x.Id == _currentUser.Id).FirstAsync(ct);
        if (curUser.IsNull())
        {
            return NotFound();
        }

        if (!BCryptHelper.Verify(oldPassword, curUser.Password))
        {
            return OperateResult.Error("旧密码错误");
        }

        //设置用户密码
        curUser.Password = BCryptHelper.Hash(newPassword);
        curUser.PasswordReSetTime = DateTime.Now;
        var isTrue = await UpdateAsync(curUser, x => new { x.Password, x.PasswordReSetTime }, ct: ct);
        if (isTrue)
        {
            await ClearUserCache(curUser.Id);

            //修改密码后撤销该用户全部设备上的会话。
            await _unitOfWork.AfterCommitAsync(() => _sessions.RevokeUserAsync(curUser.Id, CancellationToken.None));
        }

        return OperateResult.Result(isTrue);
    }


    /// <inheritdoc />
    [Authorize]
    public async Task<OperateResult> UpdatePreferencesConfigAsync(
        UpdateUserPreferencesConfigDto updateUserPreferencesConfigDto, CancellationToken ct = default)
    {
        var curUser = await TableWhere(x => x.Id == _currentUser.Id).FirstAsync(ct);
        if (curUser.IsNull())
        {
            return NotFound();
        }

        curUser.PreferencesConfig = updateUserPreferencesConfigDto.PreferencesConfig;
        var isTrue = await UpdateAsync(curUser, x => new { x.PreferencesConfig }, ct: ct);
        if (isTrue)
        {
            await ClearUserCache(curUser.Id);
        }

        return OperateResult.Result(isTrue);
    }


    /// <inheritdoc />
    [Authorize]
    public async Task<OperateResult> UpdateEmailAsync(UpdateUserEmailDto updateUserEmailDto, CancellationToken ct = default)
    {
        var curUser = await TableWhere(x => x.Id == _currentUser.Id).FirstAsync(ct);
        if (curUser.IsNull())
        {
            return NotFound();
        }

        if (!_passwordDecryptor.TryDecrypt(updateUserEmailDto.Password, out var password))
        {
            return OperateResult.Error("密码格式错误");
        }

        if (!BCryptHelper.Verify(password, curUser.Password))
        {
            return OperateResult.Error("密码错误");
        }

        var code = await _cache.GetAsync<string>(
            GlobalConstants.CachePrefix.EmailCaptcha + updateUserEmailDto.Email);
        if (code.IsNullOrEmpty() || !code.Equals(updateUserEmailDto.Code))
        {
            return OperateResult.Error("验证码已过期");
        }

        curUser.Email = updateUserEmailDto.Email;
        var result = await UpdateAsync(curUser, x => new { x.Email }, ct: ct);
        if (result) await ClearUserCache(curUser.Id);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [Authorize]
    public async Task<OperateResult> UpdateAvatarAsync(UploadFile file, CancellationToken ct = default)
    {
        if (file.Length <= 0)
        {
            return OperateResult.Error("不能上传空文件");
        }

        var fileLimitSizeMb = _systemOptions.FileLimitSize;
        if (file.Length > fileLimitSizeMb * 1024L * 1024L)
        {
            return OperateResult.Error($"头像过大，请选择小于等于 {fileLimitSizeMb}MB 的图片");
        }

        if (!FileUploadPolicy.IsAllowedAvatarFileName(file.FileName))
        {
            return OperateResult.Error("头像仅支持 jpg、jpeg、png、gif、bmp、webp 格式");
        }

        var curUser = await TableWhere(x => x.Id == _currentUser.Id).FirstAsync(ct);
        if (curUser.IsNull())
        {
            return NotFound();
        }


        var stored = await _fileStorage.SaveAsync(file, "avatar", ct);
        curUser.AvatarPath = stored.Path;
        var saved = false;
        try
        {
            saved = await UpdateAsync(curUser, ct: ct);
            if (saved) await ClearUserCache(curUser.Id);
            return OperateResult.Result(saved);
        }
        finally
        {
            if (!saved) _fileStorage.Delete(stored.Path);
        }
    }

    #endregion


    #region 扩展修改

    /// <inheritdoc />
    [UseTran]
    [HasRole(RoleCodes.Admin)]
    public async Task<OperateResult> UpdateRoleAsync(UpdateUserRoleDto updateUserRole, CancellationToken ct = default)
    {
        var user = await TableWhere(x => x.Id == updateUserRole.Id).FirstAsync(ct);
        if (user.IsNull())
        {
            return NotFound();
        }

        await UpdateAsync(user, ct: ct);
        await SugarClient.Deleteable<UserRole>().Where(x => x.UserId == updateUserRole.Id).ExecuteCommandAsync(ct);
        var userRoles = new List<UserRole>();
        userRoles.AddRange(updateUserRole.RoleIdArray.Select(r => new UserRole
        {
            UserId = updateUserRole.Id,
            RoleId = r
        }));
        await SugarClient.Insertable(userRoles).ExecuteCommandAsync(ct);
        await ClearUserCache(updateUserRole.Id);
        return OperateResult.Success();
    }


    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Users.Edit)]
    public async Task<OperateResult> UpdatePositionAsync(UpdateUserPositionDto updateUserPosition, CancellationToken ct = default)
    {
        var user = await TableWhere(x => x.Id == updateUserPosition.Id).FirstAsync(ct);
        if (user.IsNull())
        {
            return NotFound();
        }

        await UpdateAsync(user, ct: ct);
        await SugarClient.Deleteable<UserPosition>().Where(x => x.UserId == updateUserPosition.Id).ExecuteCommandAsync(ct);
        var userRoles = new List<UserPosition>();
        userRoles.AddRange(updateUserPosition.PositionIdArray.Select(r => new UserPosition { UserId = updateUserPosition.Id, PositionId = r }));
        await SugarClient.Insertable(userRoles).ExecuteCommandAsync(ct);
        await ClearUserCache(updateUserPosition.Id);
        return OperateResult.Success();
    }

    #endregion

    #region 用户缓存

    private Task ClearUserCache(long userId) => _permissionCache.UserChangedAsync(userId);

    #endregion

    #region 条件模型

    private Task<List<IConditionalModel>> GetConditionalModel(UserQueryCriteria userQueryCriteria)
    {
        if (!userQueryCriteria.DepartmentIdArray.IsNullOrEmpty())
        {
            userQueryCriteria.DepartmentIdArray = string.Join(",", userQueryCriteria.DepartmentIdArray);
        }

        return Task.FromResult(userQueryCriteria.ApplyQueryConditionalModel());
    }

    #endregion
}

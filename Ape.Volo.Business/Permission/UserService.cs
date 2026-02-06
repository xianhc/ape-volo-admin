using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.Permission.User;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission.User;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.User;
using Ape.Volo.ViewModel.Report.Permission;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace Ape.Volo.Business.Permission;

/// <summary>
/// 用户服务
/// </summary>
public class UserService : BaseServices<User>, IUserService
{
    #region 字段

    private readonly IDepartmentService _departmentService;
    private readonly IRoleService _roleService;

    #endregion

    #region 构造函数

    /// <summary>
    /// 
    /// </summary>
    /// <param name="departmentService"></param>
    /// <param name="roleService"></param>
    public UserService(IDepartmentService departmentService, IRoleService roleService)
    {
        _departmentService = departmentService;
        _roleService = roleService;
    }

    #endregion

    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> CreateAsync(CreateUpdateUserDto createUpdateUserDto)
    {
        if (await TableWhere(x => x.UserName == createUpdateUserDto.UserName).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateUserDto,
                nameof(createUpdateUserDto.UserName)));
        }

        if (await TableWhere(x => x.Email == createUpdateUserDto.Email).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateUserDto,
                nameof(createUpdateUserDto.Email)));
        }

        if (await TableWhere(x => x.Phone == createUpdateUserDto.Phone).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateUserDto,
                nameof(createUpdateUserDto.Phone)));
        }

        var user = App.Mapper.MapTo<User>(createUpdateUserDto);

        //设置用户密码
        user.Password = BCryptHelper.Hash(App.GetOptions<SystemOptions>().UserDefaultPassword);
        user.DeptId = user.Dept.Id;
        //用户
        await AddAsync(user);


        await SugarClient.Deleteable<UserRole>().Where(x => x.UserId == user.Id).ExecuteCommandAsync();
        var userRoles = new List<UserRole>();
        userRoles.AddRange(user.Roles.Select(x => new UserRole { UserId = user.Id, RoleId = x.Id }));
        await SugarClient.Insertable(userRoles).ExecuteCommandAsync();


        await SugarClient.Deleteable<UserJob>().Where(x => x.UserId == user.Id).ExecuteCommandAsync();
        var userJobs = new List<UserJob>();
        userJobs.AddRange(user.Jobs.Select(x => new UserJob { UserId = user.Id, JobId = x.Id }));
        await SugarClient.Insertable(userJobs).ExecuteCommandAsync();

        return OperateResult.Success();
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> UpdateAsync(CreateUpdateUserDto createUpdateUserDto)
    {
        //取出待更新数据
        var oldUser = await TableWhere(x => x.Id == createUpdateUserDto.Id).Includes(x => x.Roles).FirstAsync();
        if (oldUser.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateUserDto, LanguageKeyConstants.User,
                nameof(createUpdateUserDto.Id)));
        }

        if (oldUser.UserName != createUpdateUserDto.UserName &&
            await TableWhere(x => x.UserName == createUpdateUserDto.UserName).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateUserDto,
                nameof(createUpdateUserDto.UserName)));
        }

        if (oldUser.Email != createUpdateUserDto.Email &&
            await TableWhere(x => x.Email == createUpdateUserDto.Email).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateUserDto,
                nameof(createUpdateUserDto.Email)));
        }

        if (oldUser.Phone != createUpdateUserDto.Phone &&
            await TableWhere(x => x.Phone == createUpdateUserDto.Phone).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateUserDto,
                nameof(createUpdateUserDto.Phone)));
        }

        //验证角色等级
        var levels = oldUser.Roles.Select(x => x.Level);
        await _roleService.VerificationUserRoleLevelAsync(levels.Min());
        var user = App.Mapper.MapTo<User>(createUpdateUserDto);
        user.DeptId = user.Dept.Id;
        //更新用户
        await UpdateAsync(user, null, x => new { x.Password, x.AvatarPath, x.PasswordReSetTime }, false);


        await SugarClient.Deleteable<UserRole>().Where(x => x.UserId == user.Id).ExecuteCommandAsync();
        var userRoles = new List<UserRole>();
        userRoles.AddRange(user.Roles.Select(x => new UserRole { UserId = user.Id, RoleId = x.Id }));
        await SugarClient.Insertable(userRoles).ExecuteCommandAsync();


        await SugarClient.Deleteable<UserJob>().Where(x => x.UserId == user.Id).ExecuteCommandAsync();
        var userJobs = new List<UserJob>();
        userJobs.AddRange(user.Jobs.Select(x => new UserJob { UserId = user.Id, JobId = x.Id }));
        await SugarClient.Insertable(userJobs).ExecuteCommandAsync();

        //清理缓存
        await ClearUserCache(user.Id);
        return OperateResult.Success();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        if (ids.Contains(App.HttpUser.Id))
        {
            return OperateResult.Error(App.L.R("Error.ForbidToDeleteYourself"));
        }

        //验证角色等级
        await _roleService.VerificationUserRoleLevelAsync(await _roleService.QueryUserRoleLevelAsync(ids));


        var users = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        foreach (var user in users)
        {
            await ClearUserCache(user.Id);
        }

        var result = await LogicDelete<User>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<UserVo>> QueryAsync(UserQueryCriteria userQueryCriteria, Pagination pagination)
    {
        var conditionalModels = await GetConditionalModel(userQueryCriteria);
        var queryOptions = new QueryOptions<User>
        {
            Pagination = pagination,
            ConditionalModels = conditionalModels,
            IsIncludes = true
        };
        var users = await TablePageAsync(queryOptions);

        return App.Mapper.MapTo<List<UserVo>>(users);
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(UserQueryCriteria userQueryCriteria)
    {
        var conditionalModels = await GetConditionalModel(userQueryCriteria);
        var users = await Table.Includes(x => x.Dept).Includes(x => x.Roles)
            .Includes(x => x.Jobs).Where(conditionalModels).ToListAsync();
        List<ExportBase> userExports = new List<ExportBase>();
        userExports.AddRange(users.Select(x => new UserExport
        {
            Id = x.Id,
            Username = x.UserName,
            Role = string.Join(",", x.Roles.Select(r => r.Name).ToArray()),
            NickName = x.NickName,
            Phone = x.Phone,
            Email = x.Email,
            Enabled = x.Enabled,
            Dept = x.Dept.Name,
            Job = string.Join(",", x.Jobs.Select(j => j.Name).ToArray()),
            GenderCode = x.GenderCode,
            CreateTime = x.CreateTime
        }));
        return userExports;
    }

    #endregion

    #region 扩展方法

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <returns></returns>
    [UseCache(Expiration = 60, KeyPrefix = GlobalConstants.CachePrefix.UserInfoById)]
    public async Task<UserVo> QueryByIdAsync(long userId)
    {
        var user = await TableWhere(x => x.Id == userId, null, null, null, true).Includes(x => x.Dept)
            .Includes(x => x.Roles).Includes(x => x.Jobs).FirstAsync();

        return App.Mapper.MapTo<UserVo>(user);
    }

    /// <summary>
    /// 查询用户
    /// </summary>
    /// <param name="userName">邮箱 or 用户名</param>
    /// <returns></returns>
    public async Task<UserVo> QueryByNameAsync(string userName)
    {
        User user;
        if (userName.IsEmail())
        {
            user = await TableWhere(s => s.Email == userName, null, null, null, true).FirstAsync();
        }
        else
        {
            user = await TableWhere(s => s.UserName == userName, null, null, null, true).FirstAsync();
        }

        return App.Mapper.MapTo<UserVo>(user);
    }

    /// <summary>
    /// 根据部门ID查找用户
    /// </summary>
    /// <param name="deptIds"></param>
    /// <returns></returns>
    public async Task<List<UserVo>> QueryByDeptIdsAsync(List<long> deptIds)
    {
        return App.Mapper.MapTo<List<UserVo>>(
            await TableWhere(u => deptIds.Contains(u.DeptId)).ToListAsync());
    }

    /// <summary>
    /// 更新用户公共信息
    /// </summary>
    /// <param name="updateUserCenterDto"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<OperateResult> UpdateCenterAsync(UpdateUserCenterDto updateUserCenterDto)
    {
        var user = await TableWhere(x => x.Id == App.HttpUser.Id).FirstAsync();
        if (user.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var checkUser = await TableWhere(x =>
            x.Phone == updateUserCenterDto.Phone && x.Id != App.HttpUser.Id).FirstAsync();
        if (checkUser.IsNotNull())
        {
            return OperateResult.Error(ValidationError.IsExist(updateUserCenterDto,
                nameof(updateUserCenterDto.Phone)));
        }

        user.NickName = updateUserCenterDto.NickName;
        user.GenderCode = updateUserCenterDto.GenderCode;
        user.Phone = updateUserCenterDto.Phone;
        var result = await UpdateAsync(user, x => new
        {
            x.NickName,
            x.GenderCode,
            x.Phone
        });
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新用户密码
    /// </summary>
    /// <param name="userPassDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdatePasswordAsync(UpdateUserPassDto userPassDto)
    {
        var rsaOptions = App.GetOptions<RsaOptions>();
        var rsaHelper = new RsaHelper(rsaOptions.PrivateKey, rsaOptions.PublicKey);
        string oldPassword = rsaHelper.Decrypt(userPassDto.OldPassword);
        string newPassword = rsaHelper.Decrypt(userPassDto.NewPassword);
        string confirmPassword = rsaHelper.Decrypt(userPassDto.ConfirmPassword);

        if (oldPassword == newPassword)
            return OperateResult.Error(App.L.R("Error.PasswordSameAsOld"));

        if (!newPassword.Equals(confirmPassword))
        {
            return OperateResult.Error(App.L.R("Error.InputsDoNotMatch"));
        }

        var curUser = await TableWhere(x => x.Id == App.HttpUser.Id).FirstAsync();
        if (curUser.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        if (!BCryptHelper.Verify(oldPassword, curUser.Password))
        {
            return OperateResult.Error(App.L.R("Error.IncorrectOldPassword"));
        }

        //设置用户密码
        curUser.Password = BCryptHelper.Hash(newPassword);
        curUser.PasswordReSetTime = DateTime.Now;
        var isTrue = await UpdateAsync(curUser, x => new { x.Password, x.PasswordReSetTime });
        if (isTrue)
        {
            //清理缓存
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserInfoById +
                                        curUser.Id.ToString().ToMd5String16());

            //退出当前用户
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.OnlineKey +
                                        App.HttpUser.JwtToken.ToMd5String16());
        }

        return OperateResult.Success();
    }

    /// <summary>
    /// 修改邮箱
    /// </summary>
    /// <param name="updateUserEmailDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateEmailAsync(UpdateUserEmailDto updateUserEmailDto)
    {
        var curUser = await TableWhere(x => x.Id == App.HttpUser.Id).FirstAsync();
        if (curUser.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var rsaOptions = App.GetOptions<RsaOptions>();
        var rsaHelper = new RsaHelper(rsaOptions.PrivateKey, rsaOptions.PublicKey);
        string password = rsaHelper.Decrypt(updateUserEmailDto.Password);
        if (!BCryptHelper.Verify(password, curUser.Password))
        {
            return OperateResult.Error(App.L.R("Error.InvalidPassword"));
        }

        var code = await App.Cache.GetAsync<string>(
            GlobalConstants.CachePrefix.EmailCaptcha + updateUserEmailDto.Email.ToMd5String16());
        if (code.IsNullOrEmpty() || !code.Equals(updateUserEmailDto.Code))
        {
            return OperateResult.Error(App.L.R("Error.InvalidVerificationCode"));
        }

        curUser.Email = updateUserEmailDto.Email;
        var result = await UpdateAsync(curUser, x => x.Email);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新用户头像
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAvatarAsync(IFormFile file)
    {
        var curUser = await TableWhere(x => x.Id == App.HttpUser.Id).FirstAsync();
        if (curUser.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }


        var prefix = App.WebHostEnvironment.WebRootPath;
        string avatarName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + IdHelper.NextId() +
                            file.FileName.Substring(Math.Max(file.FileName.LastIndexOf('.'), 0));
        string avatarPath = Path.Combine(prefix, "uploads", "file", "avatar");

        if (!Directory.Exists(avatarPath))
        {
            Directory.CreateDirectory(avatarPath);
        }

        avatarPath = Path.Combine(avatarPath, avatarName);
        await using (var fs = new FileStream(avatarPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(fs);
            fs.Flush();
        }

        string relativePath = Path.GetRelativePath(prefix, avatarPath);
        relativePath = "/" + relativePath.Replace("\\", "/");
        curUser.AvatarPath = relativePath;
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserInfoById +
                                    curUser.Id.ToString().ToMd5String16());
        var result = await UpdateAsync(curUser);
        return OperateResult.Result(result);
    }

    #endregion


    #region 扩展修改

    /// <summary>
    /// 修改角色
    /// </summary>
    /// <param name="updateUserRole"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> UpdateRoleAsync(UpdateUserRole updateUserRole)
    {
        var user = await TableWhere(x => x.Id == updateUserRole.Id).FirstAsync();
        if (user.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        await UpdateAsync(user);
        await SugarClient.Deleteable<UserRole>().Where(x => x.UserId == updateUserRole.Id).ExecuteCommandAsync();
        var userRoles = new List<UserRole>();
        userRoles.AddRange(updateUserRole.RoleIdArray.Select(r => new UserRole
        { UserId = updateUserRole.Id, RoleId = r }));
        await SugarClient.Insertable(userRoles).ExecuteCommandAsync();
        return OperateResult.Success();
    }


    /// <summary>
    /// 修改岗位
    /// </summary>
    /// <param name="updateUserJob"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> UpdateRoleAsync(UpdateUserJob updateUserJob)
    {
        var user = await TableWhere(x => x.Id == updateUserJob.Id).FirstAsync();
        if (user.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        await UpdateAsync(user);
        await SugarClient.Deleteable<UserJob>().Where(x => x.UserId == updateUserJob.Id).ExecuteCommandAsync();
        var userRoles = new List<UserJob>();
        userRoles.AddRange(updateUserJob.JobIdArray.Select(r => new UserJob
        { UserId = updateUserJob.Id, JobId = r }));
        await SugarClient.Insertable(userRoles).ExecuteCommandAsync();
        return OperateResult.Success();
    }

    #endregion

    #region 用户缓存

    private async Task ClearUserCache(long userId)
    {
        //清理缓存
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserInfoById +
                                    userId.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(
            GlobalConstants.CachePrefix.UserAuthUrls + userId.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(
            GlobalConstants.CachePrefix.UserAuthCodes + userId.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserMenuById +
                                    userId.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserDataScopeById +
                                    userId.ToString().ToMd5String16());
    }

    #endregion

    #region 条件模型

    private Task<List<IConditionalModel>> GetConditionalModel(UserQueryCriteria userQueryCriteria)
    {
        // if (userQueryCriteria.DeptId > 0)
        // {
        //     var allIds = await _departmentService.GetChildIds([userQueryCriteria.DeptId], null);
        //     if (allIds.Any())
        //     {
        //         userQueryCriteria.DeptIdItems = string.Join(",", allIds);
        //     }
        // }
        if (!userQueryCriteria.DepartmentIdArray.IsNullOrEmpty())
        {
            userQueryCriteria.DepartmentIdArray = string.Join(",", userQueryCriteria.DepartmentIdArray);
        }

        return Task.FromResult(userQueryCriteria.ApplyQueryConditionalModel());
    }

    #endregion
}

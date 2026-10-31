using Ape.Volo.Application.Contracts.Abstractions.Storage;
using Ape.Volo.Application.Contracts.Permission.Users.Models;
using Ape.Volo.Shared.Exceptions;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Permission.Users;

/// <summary>
/// 用户接口
/// </summary>
public interface IUserService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateUserDto createUpdateUserDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateUserDto createUpdateUserDto, CancellationToken ct = default);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<UserVo>> GetListAsync(UserQueryCriteria userQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(UserQueryCriteria userQueryCriteria, CancellationToken ct = default);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 查找用户
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户</returns>
    /// <exception cref="BadRequestException">用户不存在</exception>
    Task<UserVo> QueryByIdAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 查找用户
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户，不存在时返回 null</returns>
    [RemoteService(false)]
    Task<UserVo?> QueryByNameAsync(string userName, CancellationToken ct = default);


    /// <summary>
    /// 根据部门ID查找用户
    /// </summary>
    /// <param name="deptIds"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<UserVo>> QueryByDeptIdsAsync(List<long> deptIds, CancellationToken ct = default);

    /// <summary>
    /// 修改个人中心信息
    /// </summary>
    /// <param name="updateUserCenterDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateCenterAsync(UpdateUserCenterDto updateUserCenterDto, CancellationToken ct = default);

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="userPassDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdatePasswordAsync(UpdateUserPassDto userPassDto, CancellationToken ct = default);


    /// <summary>
    /// 修改密码UpdatePreferencesConfigAsync
    /// </summary>
    /// <param name="updateUserPreferencesConfig"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdatePreferencesConfigAsync(UpdateUserPreferencesConfigDto updateUserPreferencesConfig, CancellationToken ct = default);


    /// <summary>
    /// 修改邮箱
    /// </summary>
    /// <param name="updateUserEmailDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateEmailAsync(UpdateUserEmailDto updateUserEmailDto, CancellationToken ct = default);

    /// <summary>
    /// 修改头像
    /// </summary>
    /// <param name="file"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> UpdateAvatarAsync(UploadFile file, CancellationToken ct = default);

    #endregion

    #region 扩展修改

    /// <summary>
    /// 修改角色
    /// </summary>
    /// <param name="updateUserRole"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateRoleAsync(UpdateUserRoleDto updateUserRole, CancellationToken ct = default);


    /// <summary>
    /// 修改岗位
    /// </summary>
    /// <param name="updateUserPosition"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdatePositionAsync(UpdateUserPositionDto updateUserPosition, CancellationToken ct = default);

    #endregion
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.Permission.User;
using Ape.Volo.SharedModel.Dto.Core.Permission.User;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.User;
using Microsoft.AspNetCore.Http;

namespace Ape.Volo.IBusiness.Permission;

/// <summary>
/// 用户接口
/// </summary>
public interface IUserService : IBaseServices<User>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateUserDto createUpdateUserDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateUserDto createUpdateUserDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<UserVo>> QueryAsync(UserQueryCriteria userQueryCriteria, Pagination pagination);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(UserQueryCriteria userQueryCriteria);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 查找用户
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    Task<UserVo> QueryByIdAsync(long userId);

    /// <summary>
    /// 查找用户
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <returns>用户实体</returns>
    Task<UserVo> QueryByNameAsync(string userName);


    /// <summary>
    /// 根据部门ID查找用户
    /// </summary>
    /// <param name="deptIds"></param>
    /// <returns></returns>
    Task<List<UserVo>> QueryByDeptIdsAsync(List<long> deptIds);

    /// <summary>
    /// 修改个人中心信息
    /// </summary>
    /// <param name="updateUserCenterDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateCenterAsync(UpdateUserCenterDto updateUserCenterDto);

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="userPassDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdatePasswordAsync(UpdateUserPassDto userPassDto);


    /// <summary>
    /// 修改邮箱
    /// </summary>
    /// <param name="updateUserEmailDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateEmailAsync(UpdateUserEmailDto updateUserEmailDto);

    /// <summary>
    /// 修改头像
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAvatarAsync(IFormFile file);

    #endregion

    #region 扩展修改

    /// <summary>
    /// 修改角色
    /// </summary>
    /// <param name="updateUserRole"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateRoleAsync(UpdateUserRole updateUserRole);


    /// <summary>
    /// 修改岗位
    /// </summary>
    /// <param name="updateUserJob"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateRoleAsync(UpdateUserJob updateUserJob);

    #endregion
}

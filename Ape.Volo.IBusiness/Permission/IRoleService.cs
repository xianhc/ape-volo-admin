using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.Permission.Role;
using Ape.Volo.SharedModel.Dto.Core.Permission.Role;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.Role;

namespace Ape.Volo.IBusiness.Permission;

/// <summary>
/// 角色接口
/// </summary>
public interface IRoleService : IBaseServices<Role>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateRoleDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateRoleDto createUpdateRoleDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateRoleDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateRoleDto createUpdateRoleDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="roleQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<RoleVo>> QueryAsync(RoleQueryCriteria roleQueryCriteria, Pagination pagination);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="roleQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(RoleQueryCriteria roleQueryCriteria);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 获取全部角色
    /// </summary>
    /// <returns></returns>
    Task<List<RoleVo>> QueryAllAsync();

    /// <summary>
    /// 获取用户角色等级
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<int?> QueryUserRoleLevelAsync(HashSet<long> ids);

    /// <summary>
    /// 验证角色等级
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    Task<int> VerificationUserRoleLevelAsync(int? level);

    /// <summary>
    /// 更新角色菜单
    /// </summary>
    /// <param name="updateRoleMenuDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateRoleMenuAsync(UpdateRoleMenuDto updateRoleMenuDto);

    /// <summary>
    /// 更新角色Apis
    /// </summary>
    /// <param name="updateRoleApiDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateRoleApiAsync(UpdateRoleApiDto updateRoleApiDto);

    #endregion
}

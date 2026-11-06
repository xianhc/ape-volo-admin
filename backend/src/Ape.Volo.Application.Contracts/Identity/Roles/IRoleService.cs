using Ape.Volo.Application.Contracts.Identity.Roles.Models;
using Ape.Volo.Shared.Exceptions;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Identity.Roles;

/// <summary>
/// 角色接口
/// </summary>
public interface IRoleService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 按主键查询角色，并加载菜单、API、部门关联
    /// </summary>
    /// <param name="id">角色主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>角色</returns>
    /// <exception cref="BadRequestException">角色不存在</exception>
    Task<RoleVo> GetWithDetailsAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateRoleDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateRoleDto createUpdateRoleDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateRoleDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateRoleDto createUpdateRoleDto, CancellationToken ct = default);

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
    /// <param name="roleQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<RoleVo>> GetListAsync(RoleQueryCriteria roleQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="roleQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(RoleQueryCriteria roleQueryCriteria, CancellationToken ct = default);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 获取全部角色
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<RoleVo>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// 更新角色菜单
    /// </summary>
    /// <param name="updateRoleMenuDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateRoleMenuAsync(UpdateRoleMenuDto updateRoleMenuDto, CancellationToken ct = default);

    #endregion
}

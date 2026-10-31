using Ape.Volo.Application.Contracts.Permission.Departments.Models;
using Ape.Volo.Shared.Exceptions;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Permission.Departments;

/// <summary>
/// 部门接口
/// </summary>
public interface IDepartmentService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateDepartmentDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateDepartmentDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto, CancellationToken ct = default);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(List<long> ids, CancellationToken ct = default);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="deptQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<DepartmentVo>> GetListAsync(DepartmentQueryCriteria deptQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<DepartmentVo>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="deptQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(DepartmentQueryCriteria deptQueryCriteria, CancellationToken ct = default);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 根据父ID获取全部
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<DepartmentVo>> QueryByPIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 根据ID获取一个部门
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns>部门</returns>
    /// <exception cref="BadRequestException">部门不存在或已停用</exception>
    Task<DepartmentSmallVo> QueryByIdAsync(long id, CancellationToken ct = default);


    /// <summary>
    /// 获取子级所有部门
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<DepartmentVo>> QuerySuperiorDeptAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 获取所选部门及全部下级部门ID
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="allIds"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<long>> GetChildIds(List<long> ids, List<long>? allIds, CancellationToken ct = default);

    #endregion
}

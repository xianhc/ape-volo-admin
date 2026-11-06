using Ape.Volo.Application.Contracts.Organization.Departments.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Organization.Departments;

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
}

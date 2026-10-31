using Ape.Volo.Application.Contracts.Permission.Menus.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Permission.Menus;

/// <summary>
/// 菜单接口
/// </summary>
public interface IMenuService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateMenuDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateMenuDto createUpdateMenuDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateMenuDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateMenuDto createUpdateMenuDto, CancellationToken ct = default);

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
    /// <param name="menuQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<MenuVo>> GetListAsync(MenuQueryCriteria menuQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="menuQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(MenuQueryCriteria menuQueryCriteria, CancellationToken ct = default);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 获取所有菜单
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<MenuVo>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// 根据父ID获取菜单
    /// </summary>
    /// <param name="pid">父级ID（null 表示顶级菜单）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<MenuVo>> FindByPIdAsync(long? pid = null, CancellationToken ct = default);


    /// <summary>
    /// 构建当前用户可见的菜单树
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>菜单树</returns>
    Task<List<MenuTreeVo>> BuildTreeAsync(CancellationToken ct = default);

    /// <summary>
    /// 获取所有同级或父级菜单
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<MenuVo>> FindSuperiorAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 获取所有子菜单ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<long>> FindChildAsync(long id, CancellationToken ct = default);

    #endregion
}

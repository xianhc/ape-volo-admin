using Ape.Volo.Application.Contracts.Permission.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Permission;
using Ape.Volo.Entity.Permission.Roles;
using Ape.Volo.Entity.Permission.Users;
using SqlSugar;
using Enumerable = System.Linq.Enumerable;

namespace Ape.Volo.Application.Permission;

/// <summary>
/// 数据权限服务
/// </summary>
public class DataScopeService : IDataScopeService
{
    #region 字段

    private readonly ISugarQueryProvider _sugarQueries;
    private readonly IPermissionCacheService _permissionCache;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 <see cref="DataScopeService"/> 的新实例。
    /// </summary>
    /// <param name="sugarQueries">不受数据权限限制的查询入口</param>
    /// <param name="permissionCache">权限缓存入口</param>
    public DataScopeService(ISugarQueryProvider sugarQueries, IPermissionCacheService permissionCache)
    {
        _sugarQueries = sugarQueries;
        _permissionCache = permissionCache;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    public Task<bool> HasAllDataScopeAsync(long userId, CancellationToken ct = default)
    {
        // 权限元数据不受当前账号的创建范围限制，统一走独立客户端读取。
        return _permissionCache.GetOrCreateAsync(PermissionCacheItem.AllDataScope, userId, () => _sugarQueries.QueryWithoutDataScopeAsync(async db =>
        {
            var user = await db.Queryable<User>().Includes(x => x.Roles).Where(x => x.Id == userId).FirstAsync(ct);
            if (user.IsNull())
            {
                return false;
            }

            return Enumerable.Any(user.Roles ?? [], x => x.DataScopeType == DataScopeType.All);
        }, ct), ct);
    }

    /// <inheritdoc />
    public Task<List<long>> GetDataScopeUserIdsAsync(long userId, CancellationToken ct = default)
    {
        return _permissionCache.GetOrCreateAsync(PermissionCacheItem.DataScopeUsers, userId, () => _sugarQueries.QueryWithoutDataScopeAsync(async db =>
        {
            List<long> userIdList = new List<long>();
            var user = await db.Queryable<User>().Includes(x => x.Roles).Where(x => x.Id == userId).FirstAsync(ct);
            if (user.IsNull())
            {
                return userIdList;
            }

            foreach (var role in user.Roles ?? [])
            {
                userIdList.AddRange(await GetUserIds(db, role.DataScopeType, role.Id, user.DeptId, user.Id, ct));
            }

            return Enumerable.ToList(Enumerable.Distinct(userIdList));
        }, ct), ct);
    }

    /// <summary>
    /// 按角色数据范围类型解析可访问的用户主键
    /// </summary>
    /// <param name="db">只读权限查询客户端</param>
    /// <param name="dataScopeType">数据范围类型</param>
    /// <param name="roleId">角色主键</param>
    /// <param name="deptId">用户所属部门主键</param>
    /// <param name="userId">用户主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>可访问的用户主键集合</returns>
    private async Task<List<long>> GetUserIds(ISqlSugarClient db, DataScopeType dataScopeType, long roleId,
        long deptId, long userId, CancellationToken ct = default)
    {
        List<long> userIdList = new List<long>();
        switch (dataScopeType)
        {
            case DataScopeType.MySelf:
                userIdList.Add(userId);
                break;
            case DataScopeType.MyDept:
                {
                    var userList = await db.Queryable<User>().Where(x => x.DeptId == deptId).ToListAsync(ct);
                    userIdList.AddRange(Enumerable.Select(userList, x => x.Id));
                    break;
                }
            case DataScopeType.MyDeptAndBelow:
                {
                    var deptIds = await GetChildIds(db, [deptId], null, ct);
                    var userList = await db.Queryable<User>().Where(x => deptIds.Contains(x.DeptId)).ToListAsync(ct);
                    userIdList.AddRange(Enumerable.Select(userList, x => x.Id));
                    break;
                }
            case DataScopeType.Customize:
                {
                    var role = await db.Queryable<Role>().Includes(x => x.DepartmentList).Where(x => x.Id == roleId)
                        .FirstAsync(ct);
                    if (role.IsNotNull())
                    {
                        var deptIds = (role.DepartmentList ?? []).Select(x => x.Id).ToList();
                        var userList = await db.Queryable<User>().Where(x => deptIds.Contains(x.DeptId)).ToListAsync(ct);
                        userIdList.AddRange(userList.Select(x => x.Id));
                    }

                    break;
                }
        }

        return userIdList;
    }

    /// <summary>
    /// 递归获取子部门ID
    /// </summary>
    /// <param name="db">只读权限查询客户端</param>
    /// <param name="ids">部门ID集合</param>
    /// <param name="allIds">已收集的部门ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    private async Task<List<long>> GetChildIds(ISqlSugarClient db, List<long> ids, List<long>? allIds,
        CancellationToken ct = default)
    {
        allIds ??= new List<long>();

        foreach (var id in ids.Where(id => !allIds.Contains(id)))
        {
            allIds.Add(id);
            var list = await db.Queryable<Department>().Where(x => x.ParentId == id && x.Enabled).ToListAsync(ct);
            if (list.Any())
            {
                await GetChildIds(db, list.Select(x => x.Id).ToList(), allIds, ct);
            }
        }

        return allIds;
    }

    #endregion
}

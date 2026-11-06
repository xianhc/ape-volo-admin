using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Core.Repository.SugarHandler;

/// <summary>
/// 基于当前数据库作用域的「无数据权限」查询入口。
/// </summary>
/// <param name="sqlSugarClient">当前宿主的数据库作用域</param>
public sealed class SugarQueryProvider(ISqlSugarClient sqlSugarClient) : ISugarQueryProvider
{
    /// <inheritdoc />
    public async Task<TResult> QueryWithoutDataScopeAsync<TResult>(Func<ISqlSugarClient, Task<TResult>> query,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ct.ThrowIfCancellationRequested();

        // 独立客户端避免污染业务查询的过滤器状态；软删除过滤器刻意保留。
        using var db = sqlSugarClient.CopyNew();
        // CopyNew 共用 QueryFilter 实例；先复制集合再移除规则，不能修改请求客户端的过滤器。
        var filters = new QueryFilterProvider();
        filters.GetFilterList.AddRange(db.QueryFilter.GetFilterList);
        db.QueryFilter = filters;
        db.QueryFilter.Clear<IDataScopedEntity>();
        return await query(db);
    }
}

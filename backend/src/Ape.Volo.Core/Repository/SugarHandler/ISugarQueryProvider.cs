using SqlSugar;

namespace Ape.Volo.Core.Repository.SugarHandler;

/// <summary>
/// 提供「不受数据权限限制」的查询入口，用于读取权限元数据本身。
/// </summary>
/// <remarks>
/// 数据权限过滤器由 <c>SqlSugarExtensions</c> 在宿主层注册，业务层不应直接操作
/// <c>QueryFilter</c> 或自行 <c>CopyNew</c>。需要绕过过滤器时统一走本接口，
/// 由基础设施层负责创建独立客户端、清除过滤器并释放。
/// </remarks>
public interface ISugarQueryProvider
{
    /// <summary>
    /// 在清除了数据权限过滤器的独立客户端上执行查询；软删除过滤器保留。
    /// </summary>
    /// <typeparam name="TResult">查询结果类型</typeparam>
    /// <param name="query">接收独立客户端并返回查询结果</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>查询结果</returns>
    Task<TResult> QueryWithoutDataScopeAsync<TResult>(Func<ISqlSugarClient, Task<TResult>> query,
        CancellationToken ct = default);
}

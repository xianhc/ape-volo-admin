using SqlSugar;

namespace Ape.Volo.Core.Repository.SugarHandler;

/// <summary>
/// sqlSugar接口
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ISugarRepository<TEntity> where TEntity : class
{
    ISqlSugarClient SugarClient { get; }

    /// <summary>
    /// 当前请求用户主键；后台任务或匿名请求为 null
    /// </summary>
    long? CurrentUserId { get; }
}

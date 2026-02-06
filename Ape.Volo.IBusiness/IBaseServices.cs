using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.SharedModel.Queries.Common;
using SqlSugar;

namespace Ape.Volo.IBusiness;

/// <summary>
/// 业务基类 常用增删查改方法
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IBaseServices<TEntity> where TEntity : class
{
    /// <summary>
    /// sqlSugarClient
    /// </summary>
    ISqlSugarClient SugarClient { get; }


    #region 新增

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="lockString">锁</param>
    /// <returns></returns>
    Task<bool> AddAsync(TEntity entity, string lockString = "");


    /// <summary>
    /// 批量添加实体
    /// </summary>
    /// <param name="entitys">实体集合</param>
    /// <param name="lockString">锁</param>
    /// <returns></returns>
    Task<bool> AddAsync(List<TEntity> entitys, string lockString = "");

    #endregion

    #region 修改

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="isUpdateColumns">更新列</param>
    /// <param name="ignoreColumns">忽略列</param>
    /// <param name="isIgnoreNull">是否忽略NULL列更新</param>
    /// <param name="lockString">锁</param>
    /// <returns></returns>
    Task<bool> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>> isUpdateColumns = null,
        Expression<Func<TEntity, object>> ignoreColumns = null, bool isIgnoreNull = true, string lockString = "");

    /// <summary>
    /// 批量更新实体
    /// </summary>
    /// <param name="entitys">实体对象</param>
    /// <param name="isUpdateColumns">更新列</param>
    /// <param name="ignoreColumns">忽略列</param>
    /// <param name="isIgnoreNull">是否忽略NULL列更新</param>
    /// <param name="lockString">锁</param>
    /// <returns></returns>
    Task<bool> UpdateAsync(List<TEntity> entitys, Expression<Func<TEntity, object>> isUpdateColumns = null,
        Expression<Func<TEntity, object>> ignoreColumns = null, bool isIgnoreNull = true, string lockString = "");

    #endregion

    #region 删除(逻辑删除)

    /// <summary>
    /// 逻辑删除 操作的类需继承ISoftDeletedEntity
    /// </summary>
    /// <param name="exp"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<bool> LogicDelete<T>(Expression<Func<T, bool>> exp) where T : class, ISoftDeletedEntity, new();

    #endregion

    #region Queryable

    /// <summary>
    /// 泛型Queryable
    /// </summary>
    ISugarQueryable<TEntity> Table { get; }

    /// <summary>
    /// 泛型Queryable
    /// </summary>
    /// <param name="whereExpression">条件表达式</param>
    /// <param name="selectExpression">查询表达式</param>
    /// <param name="orderExpression">排序表达式</param>
    /// <param name="orderByType">排序方式</param>
    /// <param name="isClearCreateByFilter">清除创建人过滤器</param>
    /// <param name="lockString">锁</param>
    /// <param name="cacheDurationInSeconds">缓存时间(秒)</param>
    /// <param name="isSplitTable">是否分表</param>
    /// <returns></returns>
    ISugarQueryable<TEntity> TableWhere(Expression<Func<TEntity, bool>> whereExpression = null,
        Expression<Func<TEntity, TEntity>> selectExpression = null,
        Expression<Func<TEntity, object>> orderExpression = null, OrderByType? orderByType = null,
        bool isClearCreateByFilter = false, string lockString = "", int cacheDurationInSeconds = 0,
        bool isSplitTable = false);


    /// <summary>
    /// 泛型Queryable
    /// </summary>
    /// <param name="conditionalModels">条件模型</param>
    /// <param name="selectExpression">查询表达式</param>
    /// <param name="orderExpression">排序表达式</param>
    /// <param name="orderByType">排序方式</param>
    /// <param name="isClearCreateByFilter">清除创建人过滤器</param>
    /// <param name="lockString">锁</param>
    /// <param name="cacheDurationInSeconds">缓存时间(秒)</param>
    /// <param name="isSplitTable">是否分表</param>
    /// <returns></returns>
    ISugarQueryable<TEntity> TableWhere(List<IConditionalModel> conditionalModels = null,
        Expression<Func<TEntity, TEntity>> selectExpression = null,
        Expression<Func<TEntity, object>> orderExpression = null, OrderByType? orderByType = null,
        bool isClearCreateByFilter = false, string lockString = "", int cacheDurationInSeconds = 0,
        bool isSplitTable = false);

    /// <summary>
    /// 表格分页
    /// </summary>
    /// <param name="queryOptions">查询参数</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task<List<TEntity>> TablePageAsync(QueryOptions<TEntity> queryOptions);

    #endregion
}
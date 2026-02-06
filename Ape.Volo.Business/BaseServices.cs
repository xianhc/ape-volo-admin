using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.IBusiness;
using Ape.Volo.Repository.SugarHandler;
using Ape.Volo.SharedModel.Queries.Common;
using SqlSugar;

namespace Ape.Volo.Business;

/// <summary>
/// 业务实现基类
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class BaseServices<TEntity> : IBaseServices<TEntity> where TEntity : class, new()
{
    #region 字段

    /// <summary>
    /// 当前操作对象仓储
    /// </summary>
    public ISugarRepository<TEntity> SugarRepository { get; set; }

    /// <summary>
    /// sugarClient
    /// </summary>
    public ISqlSugarClient SugarClient => SugarRepository.SugarClient;

    #endregion

    #region 构造函数

    // public BaseServices(ISugarRepository<TEntity> sugarRepository)
    // {
    //     if (sugarRepository == null)
    //     {
    //         throw new ArgumentNullException(nameof(sugarRepository), "sugarRepository cannot be null");
    //     }
    //     SugarRepository = sugarRepository;
    // }

    #endregion

    #region 新增

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">实体集合</param>
    /// <param name="lockString">锁</param>
    /// <returns></returns>
    public async Task<bool> AddAsync(TEntity entity, string lockString = "")
    {
        var insert = SugarClient.Insertable(entity);
        if (!lockString.IsNullOrEmpty())
        {
            insert = insert.With(lockString);
        }

        var result = await insert.ExecuteCommandAsync();

        return result > 0;
    }

    /// <summary>
    /// 批量添加实体
    /// </summary>
    /// <param name="entitys">实体集合</param>
    /// <param name="lockString">锁</param>
    /// <returns></returns>
    public async Task<bool> AddAsync(List<TEntity> entitys, string lockString = "")
    {
        var insert = SugarClient.Insertable(entitys);
        if (!lockString.IsNullOrEmpty())
        {
            insert = insert.With(lockString);
        }

        var result = await insert.ExecuteCommandAsync();
        return result > 0;
    }

    #endregion

    #region 修改

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="isUpdateColumns">更新列</param>
    /// <param name="ignoreColumns">忽略列</param>
    /// <param name="isIgnoreNull">是否忽略NULL列更新</param>
    /// <param name="lockString">是否加锁</param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(TEntity entity,
        Expression<Func<TEntity, object>> isUpdateColumns = null,
        Expression<Func<TEntity, object>> ignoreColumns = null, bool isIgnoreNull = true, string lockString = "")
    {
        if (isUpdateColumns != null && ignoreColumns != null)
        {
            throw new Exception(App.L.R("Error.UpdateAndExcludeConflict"));
        }

        var ignoreIsDeletedFields = (Expression<Func<TEntity, object>>)(x => ((ISoftDeletedEntity)x).IsDeleted);

        ignoreColumns = ignoreColumns == null ? ignoreIsDeletedFields : ignoreColumns.AndAlso(ignoreIsDeletedFields);


        var up = SugarClient.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: isIgnoreNull);
        if (!lockString.IsNullOrEmpty())
        {
            up = up.With(lockString);
        }

        up = up.UpdateColumnsIF(isUpdateColumns != null, isUpdateColumns)
            .IgnoreColumnsIF(ignoreColumns != null, ignoreColumns);

        var result = await up.ExecuteCommandAsync();
        return result > 0;
    }

    /// <summary>
    /// 批量更新实体
    /// </summary>
    /// <param name="entitys">实体对象</param>
    /// <param name="isUpdateColumns">更新列</param>
    /// <param name="ignoreColumns">忽略列</param>
    /// <param name="isIgnoreNull">是否忽略NULL列更新</param>
    /// <param name="lockString">是否加锁</param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(List<TEntity> entitys,
        Expression<Func<TEntity, object>> isUpdateColumns = null,
        Expression<Func<TEntity, object>> ignoreColumns = null, bool isIgnoreNull = true, string lockString = "")
    {
        if (isUpdateColumns != null && ignoreColumns != null)
        {
            throw new Exception(App.L.R("Error.UpdateAndExcludeConflict"));
        }

        var ignoreIsDeletedFields = (Expression<Func<TEntity, object>>)(x => ((ISoftDeletedEntity)x).IsDeleted);

        ignoreColumns = ignoreColumns == null ? ignoreIsDeletedFields : ignoreColumns.AndAlso(ignoreIsDeletedFields);

        var up = SugarClient.Updateable(entitys).IgnoreColumns(ignoreAllNullColumns: isIgnoreNull);
        if (!lockString.IsNullOrEmpty())
        {
            up = up.With(lockString);
        }

        up = up.UpdateColumnsIF(isUpdateColumns != null, isUpdateColumns)
            .IgnoreColumnsIF(ignoreColumns != null, ignoreColumns);
        var result = await up.ExecuteCommandAsync();
        return result > 0;
    }

    #endregion

    #region 删除(逻辑删除)

    /// <summary>
    /// 逻辑删除 操作的类需继承ISoftDeletedEntity
    /// </summary>
    /// <param name="exp"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<bool> LogicDelete<T>(Expression<Func<T, bool>> exp) where T : class, ISoftDeletedEntity, new()
    {
        return await SugarClient.Updateable<T>()
            .SetColumns(it => new T { IsDeleted = true },
                true) //true 支持更新数据过滤器赋值字段一起更新
            .Where(exp).ExecuteCommandAsync() > 0;
    }

    #endregion

    #region Queryable

    /// <summary>
    /// Table
    /// </summary>
    public ISugarQueryable<TEntity> Table => SugarClient.Queryable<TEntity>();


    /// <summary>
    /// TableWhere
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
    public ISugarQueryable<TEntity> TableWhere(Expression<Func<TEntity, bool>> whereExpression,
        Expression<Func<TEntity, TEntity>> selectExpression = null,
        Expression<Func<TEntity, object>> orderExpression = null, OrderByType? orderByType = null,
        bool isClearCreateByFilter = false, string lockString = "", int cacheDurationInSeconds = 0,
        bool isSplitTable = false)
    {
        var table = Table.Where(whereExpression).WithCacheIF(cacheDurationInSeconds > 0, cacheDurationInSeconds);

        if (!lockString.IsNullOrEmpty())
        {
            table = table.With(lockString);
        }

        if (isClearCreateByFilter)
        {
            table = table.ClearFilter<ICreateByEntity>();
        }

        if (selectExpression != null)
        {
            table = table.Select(selectExpression);
        }

        if (isSplitTable)
        {
            table = table.SplitTable();
        }

        if (orderExpression != null && orderByType != null)
        {
            table = table.OrderBy(orderExpression, (OrderByType)orderByType);
        }

        return table;
    }


    /// <summary>
    /// TableWhere
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
    public ISugarQueryable<TEntity> TableWhere(List<IConditionalModel> conditionalModels,
        Expression<Func<TEntity, TEntity>> selectExpression = null,
        Expression<Func<TEntity, object>> orderExpression = null, OrderByType? orderByType = null,
        bool isClearCreateByFilter = false, string lockString = "", int cacheDurationInSeconds = 0,
        bool isSplitTable = false)
    {
        var table = Table.Where(conditionalModels).WithCacheIF(cacheDurationInSeconds > 0, cacheDurationInSeconds);
        if (!lockString.IsNullOrEmpty())
        {
            table = table.With(lockString);
        }

        if (isClearCreateByFilter)
        {
            table = table.ClearFilter<ICreateByEntity>();
        }

        if (selectExpression != null)
        {
            table = table.Select(selectExpression);
        }

        if (isSplitTable)
        {
            table = table.SplitTable();
        }

        if (orderExpression != null && orderByType != null)
        {
            table = table.OrderBy(orderExpression, (OrderByType)orderByType);
        }

        return table;
    }

    /// <summary>
    /// 表格分页
    /// </summary>
    /// <param name="queryOptions">查询参数</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<TEntity>> TablePageAsync(QueryOptions<TEntity> queryOptions)
    {
        if (queryOptions.Pagination.PageIndex < 1)
        {
            throw new Exception(App.L.R("Error.PageNumberInvalid"));
        }

        if (queryOptions.Pagination.PageSize < 1 || queryOptions.Pagination.PageSize > 100)
        {
            throw new Exception(App.L.R("Error.PageSizeInvalid"));
        }

        if (queryOptions.WhereLambda != null && queryOptions.ConditionalModels != null)
        {
            throw new Exception(App.L.R("Error.ConditionConflict"));
        }

        RefAsync<int> totalNumber = 0; //总条数
        RefAsync<int> totalPage = 0; //总页数
        var query = Table.WithCacheIF(queryOptions.CacheDurationInSeconds > 0, queryOptions.CacheDurationInSeconds);
        if (!queryOptions.LockString.IsNullOrEmpty())
        {
            query = query.With(queryOptions.LockString);
        }

        if (queryOptions.IsIncludes)
        {
            query = query.IncludesAllFirstLayer(queryOptions.IgnorePropertyNameList);
        }

        if (queryOptions.WhereLambda != null)
        {
            query = query.Where(queryOptions.WhereLambda);
        }

        if (queryOptions.ConditionalModels != null)
        {
            query = query.Where(queryOptions.ConditionalModels);
        }


        if (queryOptions.SelectExpression != null)
        {
            query = query.Select(queryOptions.SelectExpression);
        }

        if (queryOptions.IsSplitTable)
        {
            query = query.SplitTable();
        }

        if (!string.IsNullOrEmpty(queryOptions.Pagination.SortField))
        {
            //默认防注入：并且可以用StaticConfig.Check_FieldFunc重写验证机质
            // var orderList = OrderByModel.Create(
            //     new OrderByModel
            //     {
            //         FieldName = UtilMethods.ToUnderLine(queryOptions.Pagination.SortField),
            //         OrderByType = queryOptions.Pagination.OrderByType
            //     }
            // );
            var orderList = OrderByModel.Create(
                new OrderByModel
                {
                    FieldName =
                        SugarClient.EntityMaintenance.GetDbColumnName<TEntity>(queryOptions.Pagination.SortField),
                    OrderByType = queryOptions.Pagination.OrderByType
                }
            );
            query = query.OrderBy(orderList);
        }

        var list = await query.ToPageListAsync(queryOptions.Pagination.PageIndex, queryOptions.Pagination.PageSize,
            totalNumber, totalPage);
        if (totalNumber > 0 && queryOptions.Pagination.PageIndex > totalPage)
        {
            throw new Exception(App.L.R("Error.PageNumberExceedsLimit"));
        }

        queryOptions.Pagination.TotalElements = totalNumber;
        queryOptions.Pagination.TotalPages = totalPage;
        return list;
    }

    #endregion
}
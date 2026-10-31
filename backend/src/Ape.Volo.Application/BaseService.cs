using System.Linq.Expressions;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Base;
using Ape.Volo.Shared.Exceptions;
using SqlSugar;

namespace Ape.Volo.Application;

/// <summary>
/// 业务实现基类。
/// 只提供受保护的数据访问能力与通用 CRUD 实现，<b>不声明任何对外契约</b>；
/// 对外暴露哪些能力，完全由各业务接口自行声明的方法决定。
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public abstract class BaseService<TEntity> where TEntity : class, ISoftDeletedEntity, new()
{
    #region 字段

    private readonly ISugarRepository<TEntity> _sugarRepository;

    /// <summary>
    /// sugarClient
    /// </summary>
    protected ISqlSugarClient SugarClient => _sugarRepository.SugarClient;

    /// <summary>
    /// 对象映射器（构造函数注入，不再走静态定位器）
    /// </summary>
    protected IMapper Mapper { get; }

    #endregion

    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    protected BaseService(ISugarRepository<TEntity> sugarRepository, IMapper mapper)
    {
        _sugarRepository = sugarRepository ?? throw new ArgumentNullException(nameof(sugarRepository));
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    #endregion

    #region 查询

    /// <summary>
    /// 按主键查询单条
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>实体，不存在时返回 null</returns>
    protected async Task<TEntity?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await SugarClient.Queryable<TEntity>().In(id).FirstAsync(ct);
    }

    /// <summary>
    /// 按主键批量查询
    /// </summary>
    /// <param name="ids">主键集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>实体集合，主键集合为空时返回空集合</returns>
    protected async Task<List<TEntity>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        var pkValues = ids.Distinct().Select(x => (object)x).ToArray();
        if (pkValues.Length == 0)
        {
            return new List<TEntity>();
        }

        return await SugarClient.Queryable<TEntity>().In(pkValues).ToListAsync(ct);
    }

    #endregion

    #region 新增

    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="lockString">锁</param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    protected async Task<bool> AddAsync(TEntity entity, string lockString = "", CancellationToken ct = default)
    {
        var insert = SugarClient.Insertable(entity);
        if (!lockString.IsNullOrEmpty())
        {
            insert = insert.With(lockString);
        }

        var result = await insert.ExecuteCommandAsync(ct);

        return result > 0;
    }

    /// <summary>
    /// 批量添加实体
    /// </summary>
    /// <param name="entities">实体集合</param>
    /// <param name="lockString">锁</param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    protected async Task<bool> AddAsync(List<TEntity> entities, string lockString = "",
        CancellationToken ct = default)
    {
        var insert = SugarClient.Insertable(entities);
        if (!lockString.IsNullOrEmpty())
        {
            insert = insert.With(lockString);
        }

        var result = await insert.ExecuteCommandAsync(ct);
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
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    protected async Task<bool> UpdateAsync(TEntity entity,
        Expression<Func<TEntity, object>>? isUpdateColumns = null,
        Expression<Func<TEntity, object>>? ignoreColumns = null, bool isIgnoreNull = true, string lockString = "",
        CancellationToken ct = default)
    {
        var up = BuildUpdateable(entity, isUpdateColumns, ignoreColumns, isIgnoreNull, lockString);
        var result = await up.ExecuteCommandAsync(ct);
        return result > 0;
    }

    /// <summary>
    /// 批量更新实体
    /// </summary>
    /// <param name="entities">实体对象</param>
    /// <param name="isUpdateColumns">更新列</param>
    /// <param name="ignoreColumns">忽略列</param>
    /// <param name="isIgnoreNull">是否忽略NULL列更新</param>
    /// <param name="lockString">是否加锁</param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    protected async Task<bool> UpdateAsync(List<TEntity> entities,
        Expression<Func<TEntity, object>>? isUpdateColumns = null,
        Expression<Func<TEntity, object>>? ignoreColumns = null, bool isIgnoreNull = true, string lockString = "",
        CancellationToken ct = default)
    {
        var up = BuildUpdateable(entities, isUpdateColumns, ignoreColumns, isIgnoreNull, lockString);
        var result = await up.ExecuteCommandAsync(ct);
        return result > 0;
    }

    /// <summary>
    /// 构造更新对象，统一处理更新列/忽略列互斥与软删字段保护
    /// </summary>
    private IUpdateable<TEntity> BuildUpdateable(object entityOrEntities,
        Expression<Func<TEntity, object>>? isUpdateColumns,
        Expression<Func<TEntity, object>>? ignoreColumns, bool isIgnoreNull, string lockString)
    {
        if (isUpdateColumns != null && ignoreColumns != null)
        {
            throw new Exception("更新列和排除列不能同时使用");
        }

        // TEntity 已约束为 ISoftDeletedEntity：常规更新不得改写软删除标记与删除审计，删除只能走 LogicDeleteAsync。
        Expression<Func<TEntity, object>> ignoreSoftDeleteFields =
            x => new { x.IsDeleted, x.DeleterId, x.DeletionTime };
        ignoreColumns = ignoreColumns == null ? ignoreSoftDeleteFields : ignoreColumns.AndAlso(ignoreSoftDeleteFields);

        var up = entityOrEntities is List<TEntity> entities
            ? SugarClient.Updateable(entities)
            : SugarClient.Updateable((TEntity)entityOrEntities);

        up = up.IgnoreColumns(ignoreAllNullColumns: isIgnoreNull);
        if (!lockString.IsNullOrEmpty())
        {
            up = up.With(lockString);
        }

        return up.UpdateColumnsIF(isUpdateColumns != null, isUpdateColumns)
            .IgnoreColumns(ignoreColumns);
    }

    #endregion

    #region 删除(逻辑删除)

    /// <summary>
    /// 逻辑删除，实体需实现 <see cref="ISoftDeletedEntity" />
    /// </summary>
    /// <param name="exp">删除条件</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>是否成功</returns>
    protected async Task<bool> LogicDeleteAsync(Expression<Func<TEntity, bool>> exp,
        CancellationToken ct = default)
    {
        // 删除审计必须显式写入 SET 子句：SetColumns 只输出表达式里点名的列，
        // AOP 在模板实例上补的字段不会进入 SQL。
        var deleterId = _sugarRepository.CurrentUserId;
        var deletionTime = DateTime.Now;
        return await SugarClient.Updateable<TEntity>()
            .SetColumns(it => new TEntity
            {
                IsDeleted = true,
                DeleterId = deleterId,
                DeletionTime = deletionTime
            }, true)
            .Where(exp).ExecuteCommandAsync(ct) > 0;
    }

    #endregion

    #region Queryable

    /// <summary>
    /// Table
    /// </summary>
    protected ISugarQueryable<TEntity> Table => SugarClient.Queryable<TEntity>();

    /// <summary>
    /// TableWhere
    /// </summary>
    /// <param name="whereExpression">条件表达式</param>
    /// <param name="selectExpression">查询表达式</param>
    /// <param name="orderExpression">排序表达式</param>
    /// <param name="orderByType">排序方式</param>
    /// <param name="isClearDataScopeFilter">清除数据权限过滤器</param>
    /// <param name="lockString">锁</param>
    /// <param name="cacheDurationInSeconds">缓存时间(秒)</param>
    /// <param name="isSplitTable">是否分表</param>
    /// <returns></returns>
    protected ISugarQueryable<TEntity> TableWhere(Expression<Func<TEntity, bool>> whereExpression,
        Expression<Func<TEntity, TEntity>>? selectExpression = null,
        Expression<Func<TEntity, object>>? orderExpression = null, OrderByType? orderByType = null,
        bool isClearDataScopeFilter = false, string lockString = "", int cacheDurationInSeconds = 0,
        bool isSplitTable = false)
    {
        return ApplyQueryOptions(Table.Where(whereExpression), selectExpression, orderExpression, orderByType,
            isClearDataScopeFilter, lockString, cacheDurationInSeconds, isSplitTable);
    }

    /// <summary>
    /// TableWhere
    /// </summary>
    /// <param name="conditionalModels">条件模型</param>
    /// <param name="selectExpression">查询表达式</param>
    /// <param name="orderExpression">排序表达式</param>
    /// <param name="orderByType">排序方式</param>
    /// <param name="isClearDataScopeFilter">清除数据权限过滤器</param>
    /// <param name="lockString">锁</param>
    /// <param name="cacheDurationInSeconds">缓存时间(秒)</param>
    /// <param name="isSplitTable">是否分表</param>
    /// <returns></returns>
    protected ISugarQueryable<TEntity> TableWhere(List<IConditionalModel> conditionalModels,
        Expression<Func<TEntity, TEntity>>? selectExpression = null,
        Expression<Func<TEntity, object>>? orderExpression = null, OrderByType? orderByType = null,
        bool isClearDataScopeFilter = false, string lockString = "", int cacheDurationInSeconds = 0,
        bool isSplitTable = false)
    {
        return ApplyQueryOptions(Table.Where(conditionalModels), selectExpression, orderExpression, orderByType,
            isClearDataScopeFilter, lockString, cacheDurationInSeconds, isSplitTable);
    }

    /// <summary>
    /// 统一施加缓存、锁、过滤器、选择列、分表、排序
    /// </summary>
    private static ISugarQueryable<TEntity> ApplyQueryOptions(ISugarQueryable<TEntity> query,
        Expression<Func<TEntity, TEntity>>? selectExpression,
        Expression<Func<TEntity, object>>? orderExpression, OrderByType? orderByType,
        bool isClearDataScopeFilter, string lockString, int cacheDurationInSeconds, bool isSplitTable)
    {
        query = query.WithCacheIF(cacheDurationInSeconds > 0, cacheDurationInSeconds);

        if (!lockString.IsNullOrEmpty())
        {
            query = query.With(lockString);
        }

        if (isClearDataScopeFilter)
        {
            query = query.ClearFilter<IDataScopedEntity>();
        }

        if (selectExpression != null)
        {
            query = query.Select(selectExpression);
        }

        if (isSplitTable)
        {
            query = query.SplitTable();
        }

        if (orderExpression != null && orderByType != null)
        {
            query = query.OrderBy(orderExpression, (OrderByType)orderByType);
        }

        return query;
    }

    /// <summary>
    /// 表格分页
    /// </summary>
    /// <param name="queryOptions">查询参数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<List<TEntity>> TablePageAsync(QueryOptions<TEntity> queryOptions,
        CancellationToken ct = default)
    {
        // 分页对象由调用方传入，缺失属于调用错误
        var pagination = queryOptions.Pagination
                         ?? throw new ArgumentException("分页参数不能为空", nameof(queryOptions));

        if (pagination.PageIndex < 1)
        {
            throw new BadRequestException("页码必须是 >=1 的整数");
        }

        if (pagination.PageSize < 1 || pagination.PageSize > 100)
        {
            throw new BadRequestException("每页条数必须是 >=1 并且 <= 100 的整数");
        }

        if (queryOptions.WhereLambda != null && queryOptions.ConditionalModels != null)
        {
            throw new Exception("条件表达式和条件模型不能同时使用");
        }

        RefAsync<int> totalNumber = 0; //总条数
        RefAsync<int> totalPage = 0; //总页数
        var query = Table.WithCacheIF(queryOptions.CacheDurationInSeconds > 0, queryOptions.CacheDurationInSeconds);
        if (!queryOptions.LockString.IsNullOrEmpty())
        {
            query = query.With(queryOptions.LockString);
        }

        if (queryOptions.IncludePropertyNameList is { Length: > 0 } includes)
        {
            foreach (var name in includes.Distinct(StringComparer.Ordinal))
            {
                query = query.IncludesByNameString(name);
            }
        }
        else if (queryOptions.IsIncludes)
        {
            query = query.IncludesAllFirstLayer(queryOptions.IgnorePropertyNameList ?? []);
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

        // 排序契约：Sorting 形如 "字段名 asc|desc"；未传或字段不属于当前实体时退回 id desc
        string sortField = "id";
        OrderByType sortOrder = OrderByType.Desc;
        if (Pagination.TryParseSorting(pagination.Sorting, out var sortingField, out var sortingOrder))
        {
            var columnExists = SugarClient.EntityMaintenance.GetEntityInfo<TEntity>().Columns
                .Any(column => string.Equals(column.PropertyName, sortingField, StringComparison.OrdinalIgnoreCase));
            if (columnExists)
            {
                sortField = sortingField;
                sortOrder = sortingOrder;
            }
        }

        var orderList = OrderByModel.Create(
            new OrderByModel
            {
                FieldName = SugarClient.EntityMaintenance.GetDbColumnName<TEntity>(sortField),
                OrderByType = sortOrder
            }
        );
        query = query.OrderBy(orderList);

        var list = await query.ToPageListAsync(pagination.PageIndex, pagination.PageSize,
            totalNumber, totalPage, ct);
        pagination.TotalElements = totalNumber;
        return list;
    }

    /// <summary>
    /// 分页查询并映射为分页结果
    /// 注：分页参数与查询条件均取自条件对象，未传页码时按第 1 页、每页 10 条处理
    /// </summary>
    /// <param name="criteria">分页查询条件</param>
    /// <param name="configure">额外查询配置（IsIncludes / IsSplitTable / WhereLambda 等）</param>
    /// <param name="ct">取消令牌</param>
    /// <typeparam name="TDto">返回项类型</typeparam>
    /// <returns>当前页数据与总数</returns>
    protected Task<PagedResultDto<TDto>> PageAsync<TDto>(IPagedQuery criteria,
        Action<QueryOptions<TEntity>>? configure = null, CancellationToken ct = default)
    {
        return PageAsync<TDto>(criteria, criteria.ApplyQueryConditionalModel(), configure, ct);
    }

    /// <summary>
    /// 分页查询并映射为分页结果（查询条件由调用方提供）
    /// </summary>
    /// <param name="request">分页请求（PageIndex / PageSize / Sorting）</param>
    /// <param name="conditionalModels">查询条件模型</param>
    /// <param name="configure">额外查询配置（IsIncludes / IsSplitTable / WhereLambda 等）</param>
    /// <param name="ct">取消令牌</param>
    /// <typeparam name="TDto">返回项类型</typeparam>
    /// <returns>当前页数据与总数</returns>
    protected async Task<PagedResultDto<TDto>> PageAsync<TDto>(IPagedAndSortedResultRequest request,
        List<IConditionalModel>? conditionalModels, Action<QueryOptions<TEntity>>? configure = null,
        CancellationToken ct = default)
    {
        var pagination = Pagination.From(request);
        var queryOptions = new QueryOptions<TEntity>
        {
            Pagination = pagination,
            ConditionalModels = conditionalModels
        };
        configure?.Invoke(queryOptions);

        var list = await TablePageAsync(queryOptions, ct);
        return new PagedResultDto<TDto>(pagination.TotalElements, Mapper.MapTo<List<TDto>>(list));
    }

    #endregion
}

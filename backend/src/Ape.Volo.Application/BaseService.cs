using System.Linq.Expressions;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
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
public abstract class BaseService<TEntity> where TEntity : class, IEntity<long>, ISoftDeletedEntity, new()
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

    #region 业务校验

    /// <summary>
    /// 使用当前服务的实体说明返回记录不存在结果
    /// </summary>
    /// <param name="id">可选的记录主键</param>
    /// <returns>业务失败结果</returns>
    protected static OperateResult NotFound(object? id = null)
        => OperateResult.Error(ValidationError.NotExist<TEntity>(id));

    /// <summary>
    /// 使用关联实体的说明返回记录不存在结果
    /// </summary>
    /// <param name="value">关联记录的查询值</param>
    /// <param name="propertyName">关联记录的查询字段，默认主键</param>
    /// <returns>业务失败结果</returns>
    protected static OperateResult NotFound<TRelated>(object? value = null, string propertyName = "Id")
        => OperateResult.Error(ValidationError.NotExist<TRelated>(value, propertyName));

    /// <summary>
    /// 使用当前实体的字段说明返回数据重复结果
    /// </summary>
    /// <param name="field">DTO 的字段访问表达式</param>
    /// <returns>业务失败结果</returns>
    protected static OperateResult AlreadyExists<TValue>(Expression<Func<TValue>> field)
        => OperateResult.Error(ValidationError.IsExist(field, typeof(TEntity)));

    /// <summary>
    /// 使用当前实体的字段说明返回必填结果
    /// </summary>
    /// <param name="field">DTO 的字段访问表达式</param>
    /// <returns>业务失败结果</returns>
    protected static OperateResult Required<TValue>(Expression<Func<TValue>> field)
        => OperateResult.Error(ValidationError.Required(field, typeof(TEntity)));

    #endregion

    #region 查询

    /// <summary>
    /// 按主键查询单条实体
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>实体，不存在时返回 null</returns>
    protected async Task<TEntity?> GetEntityByIdAsync(long id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await SugarClient.Queryable<TEntity>().In(id).FirstAsync(ct);
    }

    /// <summary>
    /// 按主键批量查询实体
    /// </summary>
    /// <param name="ids">主键集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>实体集合，主键集合为空时返回空集合</returns>
    protected async Task<List<TEntity>> GetEntitiesByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default)
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
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    protected async Task<bool> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        var result = await SugarClient.Insertable(entity).ExecuteCommandAsync(ct);
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
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    protected async Task<bool> UpdateAsync(TEntity entity,
        Expression<Func<TEntity, object>>? isUpdateColumns = null,
        Expression<Func<TEntity, object>>? ignoreColumns = null, bool isIgnoreNull = true,
        CancellationToken ct = default)
    {
        var up = BuildUpdateable(entity, isUpdateColumns, ignoreColumns, isIgnoreNull);
        var result = await up.ExecuteCommandAsync(ct);
        return result > 0;
    }

    /// <summary>
    /// 构造更新对象，统一处理更新列/忽略列互斥与软删字段保护
    /// </summary>
    private IUpdateable<TEntity> BuildUpdateable(TEntity entity,
        Expression<Func<TEntity, object>>? isUpdateColumns,
        Expression<Func<TEntity, object>>? ignoreColumns, bool isIgnoreNull)
    {
        if (isUpdateColumns != null && ignoreColumns != null)
        {
            throw new Exception("更新列和排除列不能同时使用");
        }

        // TEntity 已约束为 ISoftDeletedEntity：常规更新不得改写软删除标记与删除审计，删除只能走 LogicDeleteAsync。
        Expression<Func<TEntity, object>> ignoreSoftDeleteFields =
            x => new { x.IsDeleted, x.DeleterId, x.DeletionTime };
        ignoreColumns = ignoreColumns == null ? ignoreSoftDeleteFields : ignoreColumns.AndAlso(ignoreSoftDeleteFields);

        var up = SugarClient.Updateable(entity);

        up = up.IgnoreColumns(ignoreAllNullColumns: isIgnoreNull);
        // 对象更新不支持 EnableQueryFilter，必须把当前可见主键作为同一条 UPDATE 的约束。
        // DISTINCT 派生表避免 MySQL 将同表读取合并回更新目标；SQL 和参数均由 ORM 生成。
        var visibleIds = Table.Where(row => row.Id == entity.Id).Select(row => row.Id).Distinct();
        var visibleSql = visibleIds.ToSql();
        var idColumn = visibleIds.SqlBuilder.GetTranslationColumnName(
            SugarClient.EntityMaintenance.GetDbColumnName<TEntity>(nameof(IEntity<long>.Id)));
        return up.UpdateColumnsIF(isUpdateColumns != null, isUpdateColumns)
            .IgnoreColumns(ignoreColumns)
            .Where($"{idColumn} IN (SELECT {idColumn} FROM ({visibleSql.Key}) AS permitted_rows)", visibleSql.Value);
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
        return await SugarClient.Updateable<TEntity>().EnableQueryFilter()
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
    /// <param name="orderExpression">排序表达式</param>
    /// <param name="orderByType">排序方式</param>
    /// <param name="isSplitTable">是否分表</param>
    /// <returns></returns>
    protected ISugarQueryable<TEntity> TableWhere(Expression<Func<TEntity, bool>> whereExpression,
        Expression<Func<TEntity, object>>? orderExpression = null, OrderByType orderByType = OrderByType.Asc,
        bool isSplitTable = false)
    {
        return ApplyQueryOptions(Table.Where(whereExpression), orderExpression, orderByType, isSplitTable);
    }

    /// <summary>
    /// TableWhere
    /// </summary>
    /// <param name="conditionalModels">条件模型</param>
    /// <param name="orderExpression">排序表达式</param>
    /// <param name="orderByType">排序方式</param>
    /// <param name="isSplitTable">是否分表</param>
    /// <returns></returns>
    protected ISugarQueryable<TEntity> TableWhere(List<IConditionalModel> conditionalModels,
        Expression<Func<TEntity, object>>? orderExpression = null, OrderByType orderByType = OrderByType.Asc,
        bool isSplitTable = false)
    {
        return ApplyQueryOptions(Table.Where(conditionalModels), orderExpression, orderByType, isSplitTable);
    }

    /// <summary>
    /// 统一施加分表与排序，始终保留软删除和数据范围过滤。
    /// </summary>
    private static ISugarQueryable<TEntity> ApplyQueryOptions(ISugarQueryable<TEntity> query,
        Expression<Func<TEntity, object>>? orderExpression, OrderByType orderByType, bool isSplitTable)
    {
        if (isSplitTable)
        {
            query = query.SplitTable();
        }

        if (orderExpression != null)
        {
            query = query.OrderBy(orderExpression, orderByType);
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
        var query = Table;

        if (queryOptions.IncludePropertyNameList is { Length: > 0 } includes)
        {
            foreach (var name in includes.Distinct(StringComparer.Ordinal))
            {
                query = query.IncludesByNameString(name);
            }
        }

        if (queryOptions.WhereLambda != null)
        {
            query = query.Where(queryOptions.WhereLambda);
        }

        if (queryOptions.ConditionalModels != null)
        {
            query = query.Where(queryOptions.ConditionalModels);
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
    /// <param name="configure">额外查询配置（导航白名单、分表、条件表达式）</param>
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
    /// <param name="configure">额外查询配置（导航白名单、分表、条件表达式）</param>
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

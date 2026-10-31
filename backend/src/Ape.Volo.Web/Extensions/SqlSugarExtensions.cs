using System.Reflection;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Core.Caches.SqlSugar;
using Ape.Volo.Core.Caches;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Repository.SugarHandler;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Ape.Volo.Core.Serilog;
using Ape.Volo.Entity.Base;
using Serilog;
using SqlSugar;
using StackExchange.Profiling;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniProfiler = StackExchange.Profiling.MiniProfiler;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// SqlSugar扩展配置
/// </summary>
public static class SqlSugarExtensions
{
    /// <summary>
    /// 注册 SqlSugar 业务库与日志库连接，并挂载软删、数据权限过滤器及 AOP
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置根对象</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddSqlSugarService(this IServiceCollection services, IConfiguration configuration)
    {
        if (services.IsNull())
            throw new ArgumentNullException(nameof(services));

        services.AddHttpContextAccessor();
        services.TryAddScoped<RequestDataScope>();

        var systemOptions = configuration.GetConfigurableOptions<SystemOptions>();
        var options = configuration.GetConfigurableOptions<DataConnectionOptions>();
        var logOptions = configuration.GetConfigurableOptions<LogDataConnectionOptions>();
        if (options.ConnectionItem.Count == 0)
        {
            throw new Exception("AppSetting.json 的 DataConnection 配置错误，请检查！");
        }

        var allConnectionItem =
            options.ConnectionItem.Where(x => x.Enabled).ToList();
        if (allConnectionItem.Count == 0 || allConnectionItem.All(x => x.ConnId != systemOptions.MainDataBase))
        {
            throw new Exception($"请确认主库 ID:{systemOptions.MainDataBase} 的 Enabled 为 true");
        }

        if (!logOptions.Enabled)
        {
            throw new Exception($"请确认日志库 ID:{logOptions.ConnId} 的 Enabled 为 true");
        }

        var logConnId = logOptions.ConnId;
        if (allConnectionItem.Any(x => x.ConnId == logConnId))
        {
            throw new Exception($"日志库 ID:{logConnId} 不应同时配置在 DataConnection.ConnectionItem 中");
        }

        // 数据库依赖缓存服务，必须等容器构建完成后再创建，复用同一容器中的缓存实例。
        services.AddSingleton<ISqlSugarClient>(provider =>
            CreateSqlSugarClient(provider, allConnectionItem, logOptions, systemOptions));

        // 权限元数据读取必须绕过当前账号的创建范围，由基础设施层统一提供独立客户端
        services.AddScoped<ISugarQueryProvider, SugarQueryProvider>();
    }

    private static SqlSugarScope CreateSqlSugarClient(IServiceProvider provider, List<ConnectionItem> allConnectionItem,
        LogDataConnectionOptions logOptions, SystemOptions systemOptions)
    {
        var logConnId = logOptions.ConnId;
        var contextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
        var serilogOptions = provider.GetRequiredService<IOptions<SerilogOptions>>().Value;
        var middlewareOptions = provider.GetRequiredService<IOptions<MiddlewareOptions>>().Value;
        // 业务库来自 DataConnection.ConnectionItem，日志库来自独立的 LogDataConnection 节点
        var allConnectionConfig = allConnectionItem
            .Select(connectionItem => BuildConnectionConfig(connectionItem, systemOptions, provider))
            .ToList();
        allConnectionConfig.Add(BuildConnectionConfig(logOptions, systemOptions, provider));

        var sugar = new SqlSugarScope(allConnectionConfig,
            db =>
            {
                allConnectionConfig.Where(x => x.ConfigId.ToString() != logConnId).ForEach(config =>
                {
                    var sugarScopeProvider = db.GetConnectionScope((string)config.ConfigId);

                    #region 配置过滤器

                    //软删除
                    sugarScopeProvider.ConfiguringSoftDeletedFilter();

                    //数据权限  这个要放在最后
                    sugarScopeProvider.ConfiguringUserDataScopeFilter(contextAccessor.HttpContext?.RequestServices);

                    #endregion

                    #region 读写事件

                    sugarScopeProvider.Aop.DataExecuting = (value, entityInfo) => DataExecuting(value, entityInfo,
                        contextAccessor.HttpContext?.RequestServices.GetRequiredService<ICurrentUser>());

                    #endregion

                    #region 执行中

                    // sugarScopeProvider.Aop.OnLogExecuting = (sql, pars) => OnLogExecuting(
                    //     sugarScopeProvider,
                    //     Enum.GetName(typeof(SugarActionType), sugarScopeProvider.SugarActionType), sql, pars,
                    //     config.ConfigId.ToString());

                    #endregion

                    #region 执行结束

                    sugarScopeProvider.Aop.OnLogExecuted = (sql, pars) => OnLogExecuted(sugarScopeProvider.Ado,
                        sugarScopeProvider,
                        Enum.GetName(typeof(SugarActionType), sugarScopeProvider.SugarActionType)!, sql, pars,
                        config.ConfigId.ToString()!, serilogOptions, systemOptions, middlewareOptions,
                        contextAccessor.HttpContext?.RequestServices.GetRequiredService<ICurrentUser>());

                    #endregion
                });
            });
        return sugar;
    }

    /// <summary>
    /// 根据连接配置构建 SqlSugar 连接配置
    /// </summary>
    /// <param name="connectionItem">连接项</param>
    /// <param name="systemOptions">系统配置</param>
    /// <param name="provider">当前宿主的服务提供者</param>
    /// <returns></returns>
    private static ConnectionConfig BuildConnectionConfig(ConnectionItem connectionItem, SystemOptions systemOptions,
        IServiceProvider provider)
    {
        var connectionString = connectionItem.ConnectionString;
        if (connectionItem.DbType == DbType.Sqlite)
        {
            connectionString = "DataSource=" + Path.Combine(
                provider.GetRequiredService<IHostEnvironment>().ContentRootPath, connectionString ?? string.Empty);
        }

        return new ConnectionConfig
        {
            ConfigId = connectionItem.ConnId,
            ConnectionString = connectionString,
            DbType = connectionItem.DbType,
            LanguageType = LanguageType.Chinese,
            IsAutoCloseConnection = true,
            //IsShardSameThread = false,
            MoreSettings = new ConnMoreSettings
            {
                IsAutoRemoveDataCache = true,
                SqlServerCodeFirstNvarchar = true //sqlserver默认使用nvarchar
            },
            ConfigureExternalServices = new ConfigureExternalServices
            {
                // 成熟环境允许补列和索引，但禁止 CodeFirst 因实体差异删除已有列。
                EntityNameService = (_, entity) => entity.IsDisabledDelete = true,
                DataInfoCacheService = systemOptions.UseRedisCache
                    ? new SqlSugarRedisCache(provider.GetRequiredService<Ape.Volo.Core.Caches.Redis.RedisCache>(),
                        provider.GetRequiredService<IOptions<RedisOptions>>().Value)
                    : new SqlSugarDistributedCache(provider.GetRequiredService<IDistributedCache>()),
                EntityService = (c, p) =>
                {
                    p.DbColumnName = UtilMethods.ToUnderLine(p.DbColumnName); //字段使用驼峰转下划线，不需要请注释
                    if (connectionItem.DbType == DbType.MySql && p.DataType == "varchar(max)")
                    {
                        p.DataType = "longtext";
                    }

                    if (p.IsPrimarykey == false && new NullabilityInfoContext()
                            .Create(c).WriteState is NullabilityState.Nullable)
                    {
                        p.IsNullable = true;
                    }
                },
            }
        };
    }


    #region 读写事件

    /// <summary>
    /// 读写事件
    /// </summary>
    /// <param name="value"></param>
    /// <param name="entityInfo"></param>
    /// <param name="currentUser">执行时的请求用户；后台工作为 null</param>
    private static void DataExecuting(object value, DataFilterModel entityInfo, ICurrentUser? currentUser)
    {
        // 主键只在插入时生成；更新语句里的实体模板不参与主键分配。
        if (entityInfo.OperationType == DataFilterType.InsertByObject &&
            entityInfo.EntityValue is IEntity<long> { Id: 0 } entity)
        {
            entity.Id = IdHelper.NextId();
        }

        // 审计字段由契约驱动：实现哪个接口就填哪些字段，与继承哪个基类无关。
        var operatorId = currentUser is { Id: > 0 } ? currentUser.Id : (long?)null;

        switch (entityInfo.OperationType)
        {
            case DataFilterType.InsertByObject:
                if (entityInfo.EntityValue is ICreationAuditedEntity creationAudited)
                {
                    if (operatorId != null || creationAudited.CreationTime == DateTime.MinValue)
                    {
                        creationAudited.CreationTime = DateTime.Now;
                    }

                    if (operatorId != null)
                    {
                        creationAudited.CreatorId = operatorId;
                    }
                }

                break;

            case DataFilterType.UpdateByObject:
                if (entityInfo.EntityValue is IModificationAuditedEntity modificationAudited)
                {
                    modificationAudited.LastModificationTime = DateTime.Now;
                    if (operatorId != null)
                    {
                        modificationAudited.LastModifierId = operatorId;
                    }
                }

                // 软删除审计：首次置为已删除时记录删除人与删除时间，重复更新不覆盖。
                if (entityInfo.EntityValue is ISoftDeletedEntity { IsDeleted: true, DeletionTime: null } softDeleted)
                {
                    softDeleted.DeleterId = operatorId;
                    softDeleted.DeletionTime = DateTime.Now;
                }

                break;
        }
    }

    #endregion
    #region sql执行事件

    /// <summary>
    /// 执行结束
    /// </summary>
    /// <param name="ado"></param>
    /// <param name="sqlSugar"></param>
    /// <param name="operate"></param>
    /// <param name="sql"></param>
    /// <param name="pars"></param>
    /// <param name="configId"></param>
    /// <param name="serilogOptions">日志配置</param>
    /// <param name="systemOptions">系统配置</param>
    /// <param name="middlewareOptions">中间件配置</param>
    /// <param name="currentUser">执行时的请求用户</param>
    private static void OnLogExecuted(IAdo ado, ISqlSugarClient sqlSugar, string operate, string sql,
        SugarParameter[] pars, string configId, SerilogOptions serilogOptions, SystemOptions systemOptions,
        MiddlewareOptions middlewareOptions, ICurrentUser? currentUser)
    {
        try
        {
            if (!serilogOptions.RecordSql)
            {
                return;
            }

            var loggedSql = serilogOptions.IncludeSqlParameters
                ? UtilMethods.GetNativeSql(sql, pars)
                : sql;
            var sqlMsg =
                $"执行DB--> 操作用户:[{currentUser?.Account ?? string.Empty}] 操作类型:[{operate}] 数据库ID:[{configId}] {loggedSql} [参数数量]:{pars.Length} [耗时]:{ado.SqlExecutionTime.TotalMilliseconds:0.##}ms";
            if (systemOptions.RunMode == RunMode.Dev && middlewareOptions.MiniProfiler)
            {
                MiniProfiler.Current.CustomTiming("SQL", sqlMsg + "\r\n");
            }

            using (LoggerPropertyConfiguration.Create.AddAopSqlProperty(sqlSugar, serilogOptions))
            {
                if (ado.SqlExecutionTime.TotalMilliseconds > serilogOptions.SlowSqlThresholdMilliseconds)
                {
                    Log.Warning(sqlMsg + ",请检查并进行优化！" + "\r\n");
                }
                else
                {
                    Log.Information(sqlMsg + "\r\n");
                }
            }
        }
        catch (Exception e)
        {
            Log.Fatal("Error occured OnLogExecuting:" + e);
        }
    }

    #endregion

    #region 配置软删除过滤器

    /// <summary>
    /// 配置软删除过滤器
    /// </summary>
    private static void ConfiguringSoftDeletedFilter(this SqlSugarScopeProvider db)
    {
        db.QueryFilter.AddTableFilter<ISoftDeletedEntity>(it => it.IsDeleted == false);
    }

    #endregion

    #region 配置用户数据权限

    /// <summary>
    /// 配置用户数据权限
    /// </summary>
    /// <param name="db"></param>
    /// <param name="requestServices">当前请求作用域；后台工作为 null</param>
    private static void ConfiguringUserDataScopeFilter(this SqlSugarScopeProvider db, IServiceProvider? requestServices)
    {
        // 依赖只从同一个请求作用域获取，不为后台工作创建替代请求作用域。
        if (requestServices == null) return;
        var dataScope = requestServices.GetRequiredService<RequestDataScope>();

        // 保留快照引用，在实际生成 SQL 时读取；配置回调不再查询权限或阻塞异步调用。
        db.QueryFilter.AddTableFilter<IDataScopedEntity>(it =>
            !dataScope.IsRestricted || dataScope.AllowAll ||
            (it.CreatorId.HasValue && dataScope.UserIds.Contains(it.CreatorId.Value)));
    }

    #endregion
}

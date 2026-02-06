using System.Reflection;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Helper.Serilog;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Caches.SqlSugar;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Serilog;
using Ape.Volo.Entity.Base;
using Ape.Volo.IBusiness.Permission;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SqlSugar;
using StackExchange.Profiling;
using MiniProfiler = StackExchange.Profiling.MiniProfiler;

namespace Ape.Volo.Infrastructure.Extensions;

/// <summary>
/// SqlSugar扩展配置
/// </summary>
public static class SqlSugarExtensions
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(SqlSugarExtensions));

    public static void AddSqlSugarService(this IServiceCollection services)
    {
        if (services.IsNull())
            throw new ArgumentNullException(nameof(services));

        var systemOptions = App.GetOptions<SystemOptions>();
        var options = App.GetOptions<DataConnectionOptions>();
        if (options.ConnectionItem.Count == 0)
        {
            throw new Exception(App.L.R("Error.AppSettings.DataConnection"));
        }

        // var connectionMaster =
        //     dataConnection.ConnectionItem.Where(x => x.ConnId == configs.DefaultDataBase && x.Enabled).ToList();
        var allConnectionItem =
            options.ConnectionItem.Where(x => x.Enabled).ToList();
        if (allConnectionItem.Count == 0 || allConnectionItem.All(x => x.ConnId != systemOptions.MainDataBase))
        {
            throw new Exception(App.L.R("Error.Database.EnsureEnabled{0}",
                systemOptions.MainDataBase));
        }

        if (allConnectionItem.All(x => x.ConnId != systemOptions.LogDataBase))
        {
            throw new Exception(App.L.R("Error.LogDatabase.EnsureEnabled{0}",
                systemOptions.LogDataBase));
        }


        List<ConnectionConfig> allConnectionConfig = new List<ConnectionConfig>();
        List<SlaveConnectionConfig> slaveDbs = null; //从库列表

        foreach (var connectionItem in allConnectionItem)
        {
            if (connectionItem.DbType == DbType.Sqlite)
            {
                connectionItem.ConnectionString = "DataSource=" + Path.Combine(App.WebHostEnvironment.ContentRootPath,
                    connectionItem.ConnectionString ?? string.Empty);
            }

            if (systemOptions.IsCqrs)
            {
                var connectionSlaves = options.ConnectionItem
                    .Where(x => x.DbType == connectionItem.DbType && x.ConnId != connectionItem.ConnId && x.Enabled)
                    .ToList();
                if (!connectionSlaves.Any())
                {
                    throw new Exception(App.L.R("Error.Database.SlaveEnsureEnabled{0}",
                        connectionItem.ConnId));
                }

                slaveDbs = new List<SlaveConnectionConfig>();
                connectionSlaves.ForEach(db =>
                {
                    slaveDbs.Add(new SlaveConnectionConfig
                    {
                        HitRate = db.HitRate, ConnectionString = db.ConnectionString
                    });
                });
            }

            var masterDb = new ConnectionConfig
            {
                ConfigId = connectionItem.ConnId,
                ConnectionString = connectionItem.ConnectionString,
                DbType = connectionItem.DbType,
                LanguageType = LanguageType.Chinese,
                IsAutoCloseConnection = true,
                //IsShardSameThread = false,
                MoreSettings = new ConnMoreSettings
                {
                    IsAutoRemoveDataCache = true, SqlServerCodeFirstNvarchar = true //sqlserver默认使用nvarchar
                },
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    DataInfoCacheService = systemOptions.UseRedisCache
                        ? new SqlSugarRedisCache()
                        : new SqlSugarDistributedCache(),
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
                },
                // 从库
                SlaveConnectionConfigs = slaveDbs
            }; //主库
            allConnectionConfig.Add(masterDb);
        }

        var sugar = new SqlSugarScope(allConnectionConfig,
            db =>
            {
                allConnectionConfig.Where(x => x.ConfigId.ToString() != systemOptions.LogDataBase).ForEach(config =>
                {
                    var sugarScopeProvider = db.GetConnectionScope((string)config.ConfigId);

                    #region 配置过滤器

                    //软删除
                    sugarScopeProvider.ConfiguringSoftDeletedFilter();

                    //租户
                    sugarScopeProvider.ConfiguringTenantFilter();

                    //数据权限  这个要放在最后
                    sugarScopeProvider.ConfiguringUserDataScopeFilter();

                    #endregion

                    #region 读写事件

                    sugarScopeProvider.Aop.DataExecuting = DataExecuting;

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
                        Enum.GetName(typeof(SugarActionType), sugarScopeProvider.SugarActionType), sql, pars,
                        config.ConfigId.ToString());

                    #endregion
                });
            });
        services.AddSingleton<ISqlSugarClient>(sugar);
    }


    #region 读写事件

    /// <summary>
    /// 读写事件
    /// </summary>
    /// <param name="value"></param>
    /// <param name="entityInfo"></param>
    private static void DataExecuting(object value, DataFilterModel entityInfo)
    {
        if (entityInfo.EntityValue is RootKey<long> { Id: 0 } rootEntity)
        {
            rootEntity.Id = IdHelper.NextId();
        }

        #region BaseEntity

        if (entityInfo.EntityValue is BaseEntity baseEntity)
        {
            switch (entityInfo.OperationType)
            {
                case DataFilterType.InsertByObject:
                    {
                        if (baseEntity.CreateTime == DateTime.MinValue)
                        {
                            baseEntity.CreateTime = DateTime.Now;
                        }

                        break;
                    }
                case DataFilterType.UpdateByObject:
                    baseEntity.UpdateTime = DateTime.Now;
                    break;
            }

            if (App.HttpUser.IsNotNull() && !App.HttpUser.Account.IsNullOrEmpty())
            {
                switch (entityInfo.OperationType)
                {
                    case DataFilterType.InsertByObject:
                        {
                            if (baseEntity.CreateBy.IsNullOrEmpty())
                            {
                                baseEntity.CreateBy = App.HttpUser.Account;
                            }

                            var tenant = baseEntity as ITenantEntity;
                            if (tenant != null && App.HttpUser.TenantId > 0)
                            {
                                if (tenant.TenantId == 0)
                                {
                                    tenant.TenantId = App.HttpUser.TenantId;
                                }
                            }

                            break;
                        }
                    case DataFilterType.UpdateByObject:
                        baseEntity.UpdateBy = App.HttpUser.Account;
                        break;
                }
            }
        }

        #endregion

        #region BaseEntityNoDataScope

        if (entityInfo.EntityValue is BaseEntityNoDataScope baseEntityNoDataScope)
        {
            switch (entityInfo.OperationType)
            {
                case DataFilterType.InsertByObject:
                    {
                        if (baseEntityNoDataScope.CreateTime == DateTime.MinValue)
                        {
                            baseEntityNoDataScope.CreateTime = DateTime.Now;
                        }

                        break;
                    }
                case DataFilterType.UpdateByObject:
                    baseEntityNoDataScope.UpdateTime = DateTime.Now;
                    break;
            }

            if (App.HttpUser.IsNotNull() && !App.HttpUser.Account.IsNullOrEmpty())
            {
                switch (entityInfo.OperationType)
                {
                    case DataFilterType.InsertByObject:
                        {
                            if (baseEntityNoDataScope.CreateBy.IsNullOrEmpty())
                            {
                                baseEntityNoDataScope.CreateBy = App.HttpUser.Account;
                            }

                            var tenant = baseEntityNoDataScope as ITenantEntity;
                            if (tenant != null && App.HttpUser.TenantId > 0)
                            {
                                if (tenant.TenantId == 0)
                                {
                                    tenant.TenantId = App.HttpUser.TenantId;
                                }
                            }

                            break;
                        }
                    case DataFilterType.UpdateByObject:
                        baseEntityNoDataScope.UpdateBy = App.HttpUser.Account;
                        break;
                }
            }
        }

        #endregion
    }

    #endregion

    #region sql执行事件

    /// <summary>
    /// 执行中
    /// </summary>
    /// <param name="sqlSugar"></param>
    /// <param name="operate"></param>
    /// <param name="sql"></param>
    /// <param name="pars"></param>
    /// <param name="configId"></param>
    private static void OnLogExecuting(ISqlSugarClient sqlSugar, string operate, string sql,
        SugarParameter[] pars, string configId)
    {
        try
        {
            var serilogOptions = App.GetOptions<SerilogOptions>();
            var systemOptions = App.GetOptions<SystemOptions>();
            var middlewareOptions = App.GetOptions<MiddlewareOptions>();
            if (!serilogOptions.RecordSql)
            {
                return;
            }

            var sqlMsg =
                $"执行DB--> 操作用户:[{App.HttpUser.Account}] 操作类型:[{operate}] 数据库ID:[{configId}] {UtilMethods.GetNativeSql(sql, pars)}";
            if (systemOptions.RunMode == RunMode.Dev && middlewareOptions.MiniProfiler)
            {
                MiniProfiler.Current.CustomTiming("SQL", sqlMsg + "\r\n");
            }

            using (LoggerPropertyConfiguration.Create.AddAopSqlProperty(sqlSugar, serilogOptions))
            {
                Log.Information(sqlMsg + "\r\n");
            }
        }
        catch (Exception e)
        {
            Log.Fatal("Error occured OnLogExecuting:" + e);
        }
    }

    /// <summary>
    /// 执行结束
    /// </summary>
    /// <param name="ado"></param>
    /// <param name="sqlSugar"></param>
    /// <param name="operate"></param>
    /// <param name="sql"></param>
    /// <param name="pars"></param>
    /// <param name="configId"></param>
    private static void OnLogExecuted(IAdo ado, ISqlSugarClient sqlSugar, string operate, string sql,
        SugarParameter[] pars, string configId)
    {
        try
        {
            var serilogOptions = App.GetOptions<SerilogOptions>();
            var systemOptions = App.GetOptions<SystemOptions>();
            var middlewareOptions = App.GetOptions<MiddlewareOptions>();
            if (!serilogOptions.RecordSql)
            {
                return;
            }

            var sqlMsg =
                $"执行DB--> 操作用户:[{App.HttpUser.Account}] 操作类型:[{operate}] 数据库ID:[{configId}] {UtilMethods.GetNativeSql(sql, pars)}[耗时]:{ado.SqlExecutionTime.TotalMilliseconds}ms";
            if (systemOptions.RunMode == RunMode.Dev && middlewareOptions.MiniProfiler)
            {
                MiniProfiler.Current.CustomTiming("SQL", sqlMsg + "\r\n");
            }

            using (LoggerPropertyConfiguration.Create.AddAopSqlProperty(sqlSugar, serilogOptions))
            {
                if (ado.SqlExecutionTime.TotalMilliseconds > 10)
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

    #region 配置多租户过滤器

    /// <summary>
    /// 配置多租户过滤器
    /// </summary>
    private static void ConfiguringTenantFilter(this SqlSugarScopeProvider db)
    {
        if (App.HttpUser.IsNotNull() && App.HttpUser.TenantId > 0)
        {
            db.QueryFilter.AddTableFilter<ITenantEntity>(it => it.TenantId == App.HttpUser.TenantId);
        }
    }

    #endregion

    #region 配置用户数据权限

    /// <summary>
    /// 配置用户数据权限
    /// </summary>
    /// <param name="db"></param>
    private static void ConfiguringUserDataScopeFilter(this SqlSugarScopeProvider db)
    {
        if (App.HttpUser.IsNull() || App.HttpUser.Account.IsNullOrEmpty()) return;
        var dataScopeService = App.GetService<IDataScopeService>();
        if (dataScopeService == null) return;

        try
        {
            var accounts = AsyncHelper.RunSync(() =>
                dataScopeService.GetDataScopeAccountsAsync(App.HttpUser.Id));
            if (accounts.Count > 0)
            {
                if (accounts.Count == 1)
                {
                    if (!accounts[0].Equals("All"))
                    {
                        db.QueryFilter.AddTableFilter<ICreateByEntity>(it => it.CreateBy == accounts[0]);
                    }
                }
                else
                {
                    db.QueryFilter.AddTableFilter<ICreateByEntity>(it => accounts.Contains(it.CreateBy));
                }
            }
            else
            {
                db.QueryFilter.AddTableFilter<ICreateByEntity>(it => it.CreateBy == App.HttpUser.Account);
            }
        }
        catch (Exception e)
        {
            Logger.Fatal("Error in configuring user data permissions：\r\n" + ExceptionHelper.GetExceptionAllMsg(e));
            db.QueryFilter.AddTableFilter<ICreateByEntity>(it => it.CreateBy == App.HttpUser.Account);
        }
    }

    #endregion
}
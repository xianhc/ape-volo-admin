using System.IO;
using System.Linq;
using System.Reflection;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.WebApp;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.Repository.UnitOfWork;
using SqlSugar;

namespace Ape.Volo.Repository.SugarHandler;

/// <summary>
/// SqlSugar仓储
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class SugarRepository<TEntity> : ISugarRepository<TEntity> where TEntity : class, new()
{
    public SugarRepository(IUnitOfWork unitOfWork, IHttpUser httpUser)
    {
        var sqlSugarScope = unitOfWork.GetDbClient();
        var logDbAttribute = typeof(TEntity).GetCustomAttribute<LogDataBaseAttribute>();
        if (logDbAttribute != null)
        {
            SugarClient = sqlSugarScope.GetConnectionScope(AppSettings.GetValue<string>("System", "LogDataBase"));
            return;
        }

        //使用多租户
        var useMultiTenant = AppSettings.GetValue<bool>("Tenant", "Enabled");
        if (useMultiTenant)
        {
            var multiDbTenantAttribute = typeof(TEntity).GetCustomAttribute<MultiDbTenantAttribute>();
            if (multiDbTenantAttribute != null)
            {
                if (httpUser.IsNotNull() && httpUser.TenantId > 0)
                {
                    var tenants = sqlSugarScope.Queryable<Tenant>().WithCache(86400).ToList();
                    var tenant = tenants.FirstOrDefault(x => x.TenantId == httpUser.TenantId);
                    if (tenant != null)
                    {
                        var iTenant = sqlSugarScope.AsTenant();
                        if (!iTenant.IsAnyConnection(tenant.ConfigId))
                        {
                            var connectionString = tenant.ConnectionString;
                            if (tenant.DbType == DbType.Sqlite)
                            {
                                connectionString = "DataSource=" +
                                                   Path.Combine(AppSettings.ContentRootPath,
                                                       tenant.ConnectionString);
                            }

                            iTenant.AddConnection(TenantHelper.GetConnectionConfig(tenant.ConfigId, tenant.DbType.Value,
                                connectionString));
                        }

                        SugarClient = iTenant.GetConnectionScope(tenant.ConfigId);
                        return;
                    }
                }
            }
        }

        SugarClient = sqlSugarScope;
    }

    public ISqlSugarClient SugarClient { get; }
}

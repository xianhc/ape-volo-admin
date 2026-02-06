using System.Reflection;
using System.Text;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.ClassLibrary;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Entity.Core.Message.Email;
using Ape.Volo.Entity.Core.Permission;
using Ape.Volo.Entity.Core.Permission.Role;
using Ape.Volo.Entity.Core.Permission.User;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.Entity.Core.System.Dict;
using Newtonsoft.Json;
using SqlSugar;

namespace Ape.Volo.Core.SeedData
{
    /// <summary>
    ///  种子数据
    /// </summary>
    public class SeedService
    {
        /// <summary>
        /// 初始化系统主库
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="systemOptions"></param>
        /// <param name="tenantOptions"></param>
        /// <returns></returns>
        public static async Task InitMasterDataAsync(DataContext dataContext, SystemOptions systemOptions,
            TenantOptions tenantOptions)
        {
            try
            {
                if (!systemOptions.IsInitDb)
                {
                    return;
                }

                if (dataContext.DbType != DbType.Oracle)
                {
                    dataContext.Db.DbMaintenance.CreateDatabase();
                }
                else
                {
                    throw new Exception("不支持Oracle使用代码建库,请先建库后注释该代码重新启动！");
                }


                #region 初始化主库数据表

                var entityList = GlobalType.EntityTypes
                    .Where(x => x.GetCustomAttribute<SugarTable>() != null &&
                                !x.GetInterfaces().Contains(typeof(ITenantEntity)) &&
                                x.GetCustomAttribute<LogDataBaseAttribute>() == null &&
                                x.GetCustomAttribute<MultiDbTenantAttribute>() == null).ToList();
                if (tenantOptions.Type == TenantType.Id)
                {
                    //多租户开启且使用ID隔离模式
                    entityList.AddRange(GlobalType.EntityTypes
                        .Where(x => x.GetCustomAttribute<SugarTable>() != null &&
                                    x.GetInterfaces().Contains(typeof(ITenantEntity))));
                }

                var masterTables = dataContext.Db.DbMaintenance.GetTableInfoList();
                entityList.ForEach(entity =>
                {
                    var entityInfo = dataContext.Db.EntityMaintenance.GetEntityInfo(entity);

                    if (!masterTables.Any(x =>
                            x.Name.Equals(entityInfo.DbTableName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (entity.GetCustomAttribute<SplitTableAttribute>() != null)
                        {
                            dataContext.Db.CodeFirst.SplitTables().InitTables(entity);
                        }
                        else
                        {
                            dataContext.Db.CodeFirst.InitTables(entity);
                        }
                    }
                });

                #endregion

                #region 初始化主库数据

                JsonSerializerSettings setting = new();
                JsonConvert.DefaultSettings = () =>
                {
                    setting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    setting.NullValueHandling = NullValueHandling.Ignore;
                    setting.Converters = new List<JsonConverter> { new CurrentDateTimeConverter() };
                    return setting;
                };
                string seedDataFolder = "resources/db/{0}.json";
                seedDataFolder = Path.Combine(App.WebHostEnvironment.WebRootPath, seedDataFolder);

                #region 用户

                if (!await dataContext.Db.Queryable<User>().AnyAsync())
                {
                    var attr = typeof(User).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<User>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<User>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 角色

                if (!await dataContext.Db.Queryable<Role>().AnyAsync())
                {
                    var attr = typeof(Role).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<Role>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<Role>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 菜单

                if (!await dataContext.Db.Queryable<Menu>().AnyAsync())
                {
                    var attr = typeof(Menu).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<Menu>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<Menu>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 部门

                if (!await dataContext.Db.Queryable<Department>().AnyAsync())
                {
                    var attr = typeof(Department).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<Department>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<Department>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 岗位

                if (!await dataContext.Db.Queryable<Job>().AnyAsync())
                {
                    var attr = typeof(Job).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<Job>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<Job>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 系统参数

                if (!await dataContext.Db.Queryable<Setting>().AnyAsync())
                {
                    var attr = typeof(Setting).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<Setting>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<Setting>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 字典

                if (!await dataContext.Db.Queryable<Dict>().AnyAsync())
                {
                    var attr = typeof(Dict).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<Dict>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<Dict>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 字典详情

                if (!await dataContext.Db.Queryable<DictDetail>().AnyAsync())
                {
                    var attr = typeof(DictDetail).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<DictDetail>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<DictDetail>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 作业调度

                if (!await dataContext.Db.Queryable<QuartzNet>().AnyAsync())
                {
                    var attr = typeof(QuartzNet).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<QuartzNet>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<QuartzNet>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 邮箱账户

                if (!await dataContext.Db.Queryable<EmailAccount>().AnyAsync())
                {
                    var attr = typeof(EmailAccount).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<EmailAccount>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<EmailAccount>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 邮件模板

                if (!await dataContext.Db.Queryable<EmailMessageTemplate>().AnyAsync())
                {
                    var attr = typeof(EmailMessageTemplate).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<EmailMessageTemplate>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<EmailMessageTemplate>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 用户与角色

                if (!await dataContext.Db.Queryable<UserRole>().AnyAsync())
                {
                    var attr = typeof(UserRole).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<UserRole>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<UserRole>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 用户与岗位

                if (!await dataContext.Db.Queryable<UserJob>().AnyAsync())
                {
                    var attr = typeof(UserJob).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<UserJob>()
                            .InsertRangeAsync(JsonConvert.DeserializeObject<List<UserJob>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 角色与菜单

                if (!await dataContext.Db.Queryable<RoleMenu>().AnyAsync())
                {
                    var attr = typeof(RoleMenu).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<RoleMenu>()
                            .InsertRangeAsync(JsonConvert.DeserializeObject<List<RoleMenu>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region Apis

                if (!await dataContext.Db.Queryable<Apis>().AnyAsync())
                {
                    var attr = typeof(Apis).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<Apis>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<Apis>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 角色与Apis

                if (!await dataContext.Db.Queryable<RoleApis>().AnyAsync())
                {
                    var attr = typeof(RoleApis).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<RoleApis>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<RoleApis>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                #region 租户

                if (!await dataContext.Db.Queryable<Tenant>().AnyAsync())
                {
                    var attr = typeof(Tenant).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<Tenant>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<Tenant>>(
                                FileHelper.ReadFile(string.Format(seedDataFolder, attr.TableName), Encoding.UTF8),
                                setting));
                    }
                }

                #endregion

                ConsoleHelper.WriteLine("Master database initialization successful！", ConsoleColor.Green);
                ConsoleHelper.WriteLine();

                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// 初始化日志库
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="logDataBase"></param>
        /// <exception cref="Exception"></exception>
        public static Task InitLogDataAsync(DataContext dataContext, string logDataBase)
        {
            if (!dataContext.Db.IsAnyConnection(logDataBase))
            {
                throw new ApplicationException("未配置日志数据库，请检查DataConnection节点配置");
            }


            var logDb = dataContext.Db.GetConnectionScope(logDataBase);
            if (logDb.CurrentConnectionConfig.DbType != DbType.Oracle)
            {
                logDb.DbMaintenance.CreateDatabase();
            }
            else
            {
                throw new Exception("sqlSugar不支持Oracle使用代码建库,请先建库后注释该代码重新启动！");
            }


            var logEntityList = GlobalType.EntityTypes
                .Where(x => x.GetCustomAttribute<SugarTable>() != null &&
                            x.GetCustomAttribute<LogDataBaseAttribute>() != null).ToList();


            var logTables = logDb.DbMaintenance.GetTableInfoList();

            logEntityList.ForEach(entity =>
            {
                var entityInfo = dataContext.Db.EntityMaintenance.GetEntityInfo(entity);

                DateTime now = DateTime.Now;
                var tableName = entityInfo.DbTableName.Replace("{year}", now.Year.ToString())
                    .Replace("{month}", now.Month.ToString("D2"))
                    .Replace("{day}", "01");

                if (!logTables.Any(x => x.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (entity.GetCustomAttribute<SplitTableAttribute>() != null)
                    {
                        logDb.CodeFirst.SplitTables().InitTables(entity);
                    }
                    else
                    {
                        logDb.CodeFirst.InitTables(entity);
                    }
                }
            });
            ConsoleHelper.WriteLine("Log database initialization successful！", ConsoleColor.Green);
            ConsoleHelper.WriteLine();
            return Task.FromResult(Task.CompletedTask);
        }


        /// <summary>
        /// 初始化租户库
        /// </summary>
        /// <param name="dataContext"></param>
        /// <exception cref="Exception"></exception>
        public static async Task InitTenantDataAsync(DataContext dataContext)
        {
            var tenants = await dataContext.Db.Queryable<Tenant>().Where(s => s.TenantType == TenantType.Db)
                .ToListAsync();
            if (tenants.Count == 0)
            {
                return;
            }

            foreach (var tenant in tenants)
            {
                var iTenant = dataContext.Db.AsTenant();
                iTenant.RemoveConnection(tenant.ConfigId);
                var connectionString = tenant.ConnectionString;
                if (tenant.DbType == DbType.Sqlite)
                {
                    connectionString = "DataSource=" +
                                       Path.Combine(App.WebHostEnvironment.ContentRootPath, tenant.ConnectionString);
                }

                iTenant.AddConnection(
                    TenantHelper.GetConnectionConfig(tenant.ConfigId, tenant.DbType.Value, connectionString));
                var db = iTenant.GetConnectionScope(tenant.ConfigId);
                if (db.CurrentConnectionConfig.DbType == DbType.Oracle)
                {
                    db.DbMaintenance.CreateDatabase();
                }
                else
                {
                    throw new Exception("sqlSugar不支持Oracle使用代码建库,请先建库后注释该代码重新启动！");
                }

                var entityList = GlobalType.EntityTypes
                    .Where(x => x.GetCustomAttribute<SugarTable>() != null &&
                                x.GetCustomAttribute<MultiDbTenantAttribute>() != null).ToList();


                if (entityList.Count == 0)
                {
                    continue;
                }

                var masterTables = db.DbMaintenance.GetTableInfoList();
                entityList.ForEach(entity =>
                {
                    var entityInfo = db.EntityMaintenance.GetEntityInfo(entity);

                    if (!masterTables.Any(x =>
                            x.Name.Equals(entityInfo.DbTableName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (entity.GetCustomAttribute<SplitTableAttribute>() != null)
                        {
                            db.CodeFirst.SplitTables().InitTables(entity);
                        }
                        else
                        {
                            db.CodeFirst.InitTables(entity);
                        }
                    }
                });
            }

            ConsoleHelper.WriteLine("Tenant database initialization successful！", ConsoleColor.Green);
        }
    }
}
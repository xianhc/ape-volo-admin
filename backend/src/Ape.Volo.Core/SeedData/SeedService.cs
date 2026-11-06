using System.Reflection;
using System.Text;
using Ape.Volo.Common.Helper;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Entity.Attributes;
using Ape.Volo.Entity.Message.Email;
using Ape.Volo.Entity.Operations;
using Ape.Volo.Entity.Organization.Departments;
using Ape.Volo.Entity.Organization.Positions;
using Ape.Volo.Entity.Identity;
using Ape.Volo.Entity.Identity.Roles;
using Ape.Volo.Entity.Identity.Users;
using Ape.Volo.Entity.System;
using Ape.Volo.Entity.System.Dictionaries;
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
        /// <param name="dataContext">数据上下文</param>
        /// <param name="systemOptions">系统配置</param>
        /// <param name="contentRootPath">应用内容根目录，种子数据文件位于非公开的 resources/db</param>
        /// <returns></returns>
        public static async Task InitMasterDataAsync(DataContext dataContext, SystemOptions systemOptions,
            string contentRootPath)
        {
            try
            {
                if (!systemOptions.IsInitDb)
                {
                    return;
                }

                EnsureDatabase(dataContext.Db, "主库", "DataConnection:ConnectionItem");


                #region 初始化主库数据表

                var entityList = typeof(Ape.Volo.Entity.Base.EntityBase).Assembly.GetExportedTypes()
                    .Where(x => x.GetCustomAttribute<SugarTable>() != null &&
                                x.GetCustomAttribute<LogDataBaseAttribute>() == null).ToList();

                entityList.ForEach(entity =>
                {
                    var isSplitTable = entity.GetCustomAttribute<SplitTableAttribute>() != null;

                    if (isSplitTable)
                    {
                        // 分表按月物理创建、完全由应用维护，schema 必须跟随实体；
                        // 只在缺失时建表会让跨版本升级后的旧分表缺列，因此无条件执行（InitTables 会补齐缺失列）。
                        dataContext.Db.CodeFirst.SplitTables().InitTables(entity);
                        return;
                    }

                    // IsInitDb 是显式迁移开关；开启时为已有普通表补齐新增列和索引。
                    // EntityNameService 已全局禁止删列，避免实体差异导致数据丢失。
                    dataContext.Db.CodeFirst.InitTables(entity);
                });

                #endregion

                #region 初始化主库数据

                var setting = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss",
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = { new SeedDateTimeConverter() }
                };
                string seedDataFolder = "resources/db/{0}.json";
                seedDataFolder = Path.Combine(contentRootPath, seedDataFolder);

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

                if (!await dataContext.Db.Queryable<Position>().AnyAsync())
                {
                    var attr = typeof(Position).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<Position>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<Position>>(
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

                if (!await dataContext.Db.Queryable<ScheduledJob>().AnyAsync())
                {
                    var attr = typeof(ScheduledJob).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<ScheduledJob>().InsertRangeAsync(
                            JsonConvert.DeserializeObject<List<ScheduledJob>>(
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

                if (!await dataContext.Db.Queryable<UserPosition>().AnyAsync())
                {
                    var attr = typeof(UserPosition).GetCustomAttribute<SugarTable>();
                    if (attr != null)
                    {
                        await dataContext.GetEntityDb<UserPosition>()
                            .InsertRangeAsync(JsonConvert.DeserializeObject<List<UserPosition>>(
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

                ConsoleHelper.WriteLine("Master database initialization successful！", ConsoleColor.Green);
                ConsoleHelper.WriteLine();

                #endregion
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"主库初始化失败：{ex.Message}", ex);
            }
        }


        /// <summary>
        /// 初始化日志库
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="logConnId">日志库连接ID（LogDataConnection.ConnId）</param>
        /// <exception cref="Exception"></exception>
        public static Task InitLogDataAsync(DataContext dataContext, string logConnId)
        {
            if (!dataContext.Db.IsAnyConnection(logConnId))
            {
                throw new ApplicationException("未配置日志数据库，请检查 LogDataConnection 节点配置");
            }


            var logDb = dataContext.Db.GetConnectionScope(logConnId);
            EnsureDatabase(logDb, "日志库", "LogDataConnection");


            var logEntityList = typeof(Ape.Volo.Entity.Base.EntityBase).Assembly.GetExportedTypes()
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

                if (!logTables.Any(x => x.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase)) ||
                    entity.GetCustomAttribute<SplitTableAttribute>() != null)
                {
                    if (entity.GetCustomAttribute<SplitTableAttribute>() != null)
                    {
                        // 与主库同理：日志分表由应用维护，无条件补齐缺失列，避免旧分表缺列导致写入失败。
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

        private static void EnsureDatabase(ISqlSugarClient db, string role, string configurationPath)
        {
            if (db.CurrentConnectionConfig.DbType != DbType.Oracle)
            {
                db.DbMaintenance.CreateDatabase();
                return;
            }

            // Oracle 由 DBA 准备数据库和业务用户，应用仅初始化已有 Schema 中的表。
            try
            {
                db.Ado.CheckConnection();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Oracle{role}（连接 ID：{db.CurrentConnectionConfig.ConfigId}）连接失败。" +
                    "SqlSugar 不支持自动创建 Oracle 数据库；请确认数据库服务/PDB 和业务用户（Schema）已准备就绪，" +
                    $"账号具备建表权限，并检查 {configurationPath} 中的连接配置。无需修改初始化源码。", ex);
            }
        }
    }
}

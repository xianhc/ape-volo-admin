using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.ConfigOptions;
using SqlSugar;

namespace Ape.Volo.Core.SeedData
{
    /// <summary>
    /// 
    /// </summary>
    public class DataContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlSugarClient"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DataContext(ISqlSugarClient sqlSugarClient)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ArgumentNullException(nameof(sqlSugarClient), "The database connection string is empty");
            _db = sqlSugarClient as SqlSugarScope;
        }

        /// <summary>
        /// 
        /// </summary>
        public ConnectionItem ConnectObject => GetCurrentDataBase();

        /// <summary>
        /// 连接字符串 
        /// </summary>
        public string ConnectionString => ConnectObject.ConnectionString;

        /// <summary>
        /// 数据库类型 
        /// </summary>
        public DbType DbType => ConnectObject.DbType;


        /// <summary>
        /// 数据连接对象 
        /// </summary>
        private SqlSugarScope _db;

        public SqlSugarScope Db
        {
            get => _db;
            private set => _db = value;
        }

        // /// <summary>
        // /// 数据连接对象 
        // /// </summary>
        // private Configs _configs;
        //
        // public Configs Configs
        // {
        //     get => _configs;
        // }

        /// <summary>
        /// 当前数据库
        /// </summary>
        private ConnectionItem GetCurrentDataBase()
        {
            var defaultConnectionItem =
                App.GetOptions<DataConnectionOptions>().ConnectionItem
                    .FirstOrDefault(x => x.ConnId == App.GetOptions<SystemOptions>().MainDataBase);
            if (defaultConnectionItem.IsNull())
            {
                throw new Exception("Database configuration error, please check！");
            }

            return defaultConnectionItem;
        }

        #region 实例方法

        /// <summary>
        /// 获取数据库处理对象
        /// </summary>
        /// <returns>返回值</returns>
        public SimpleClient<T> GetEntityDb<T>() where T : class, new()
        {
            return new SimpleClient<T>(_db);
        }

        /// <summary>
        /// 获取数据库处理对象
        /// </summary>
        /// <param name="db">db</param>
        /// <returns>返回值</returns>
        public SimpleClient<T> GetEntityDb<T>(SqlSugarClient db) where T : class, new()
        {
            return new SimpleClient<T>(db);
        }

        #endregion


        #region 根据实体类生成数据库表

        /// <summary>
        /// 根据实体类生成数据库表
        /// </summary>
        /// <param name="blnBackupTable">是否备份表</param>
        /// <param name="entityList">指定的实体</param>
        public void CreateTableByEntity<T>(bool blnBackupTable, params T[] entityList) where T : class, new()
        {
            Type[] lstTypes = null;
            if (entityList != null)
            {
                lstTypes = new Type[entityList.Length];
                for (int i = 0; i < entityList.Length; i++)
                {
                    lstTypes[i] = typeof(T);
                }
            }

            CreateTableByEntity(blnBackupTable, lstTypes);
        }

        /// <summary>
        /// 根据实体类生成数据库表
        /// </summary>
        /// <param name="blnBackupTable">是否备份表</param>
        /// <param name="entityList">指定的实体</param>
        private void CreateTableByEntity(bool blnBackupTable, params Type[] entityList)
        {
            if (blnBackupTable)
            {
                Db.CodeFirst.BackupTable().InitTables(entityList); //change entity backupTable            
            }
            else
            {
                Db.CodeFirst.InitTables(entityList);
            }
        }

        #endregion
    }
}
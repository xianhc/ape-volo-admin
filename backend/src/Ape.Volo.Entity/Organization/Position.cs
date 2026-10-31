using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Permission.Users;
using SqlSugar;

namespace Ape.Volo.Entity.Organization
{
    /// <summary>
    /// 岗位
    /// </summary>
    [SugarTable("sys_position")]
    [SugarIndex("unique_{table}_Name", nameof(Name), OrderByType.Asc, true)]
    public class Position : DataScopedEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(DefaultValue = "999")]
        public int Sort { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 用户列表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(typeof(UserPosition), nameof(UserPosition.PositionId), nameof(UserPosition.UserId))]
        public List<User>? Users { get; set; }
    }
}
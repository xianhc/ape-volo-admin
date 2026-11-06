using SqlSugar;

namespace Ape.Volo.Entity.Identity.Users
{
    /// <summary>
    /// 用户岗位关联
    /// </summary>
    [SugarTable("sys_user_position")]
    [SugarIndex("index_{table}_PositionId", nameof(PositionId), OrderByType.Asc)]
    public class UserPosition
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long UserId { get; set; }

        /// <summary>
        /// 岗位ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long PositionId { get; set; }
    }
}

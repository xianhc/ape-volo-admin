using SqlSugar;

namespace Ape.Volo.Entity.Identity.Users
{
    /// <summary>
    /// 用户角色关联
    /// </summary>
    [SugarTable("sys_user_role")]
    [SugarIndex("index_{table}_RoleId", nameof(RoleId), OrderByType.Asc)]
    public class UserRole
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long RoleId { get; set; }
    }
}

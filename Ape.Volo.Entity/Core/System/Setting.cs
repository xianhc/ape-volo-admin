using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.System
{
    /// <summary>
    /// 参数配置
    /// </summary>
    [SugarTable("sys_setting")]
    [SugarIndex("unique_{table}_Name", nameof(Name), OrderByType.Asc, true)]
    public class Setting : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
    }
}
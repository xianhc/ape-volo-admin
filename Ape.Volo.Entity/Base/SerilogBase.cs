using System;
using SqlSugar;

namespace Ape.Volo.Entity.Base
{
    /// <summary>
    /// Serilog日志基类
    /// </summary>
    public class SerilogBase : RootKey<long>
    {
        /// <summary>
        /// 创建者名称
        /// </summary>
        public string? CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SplitField]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? Message { get; set; }

        /// <summary>
        /// 消息模板
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? MessageTemplate { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? Properties { get; set; }
    }
}

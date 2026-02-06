using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.Permission
{
    /// <summary>
    /// Api路由
    /// </summary>
    [SugarTable("sys_apis")]
    public class Apis : BaseEntity
    {
        /// <summary>
        /// 组
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// 路径
        /// </summary>
        public string Url { get; set; } = string.Empty;


        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 请求方法
        /// </summary>
        public string Method { get; set; } = string.Empty;
    }
}
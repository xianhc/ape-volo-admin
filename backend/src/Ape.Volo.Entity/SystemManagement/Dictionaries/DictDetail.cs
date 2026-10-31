using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.SystemManagement.Dictionaries
{
    /// <summary>
    /// 字典详情
    /// </summary>
    [SugarTable("sys_dict_detail")]
    [SugarIndex("index_{table}_DictId_DictSort", nameof(DictId), OrderByType.Asc,
        nameof(DictSort), OrderByType.Asc)]
    public class DictDetail : AuditedEntity
    {
        /// <summary>
        /// 字典ID
        /// </summary>
        [SugarColumn(IsNullable = false, IsOnlyIgnoreUpdate = true)]
        public long DictId { get; set; }

        /// <summary>
        /// 字典标签
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// 字典值
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(DefaultValue = "999")]
        public int DictSort { get; set; }
    }
}

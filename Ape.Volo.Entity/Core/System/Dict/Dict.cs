using System.Collections.Generic;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.System.Dict
{
    /// <summary>
    /// 字典
    /// </summary>
    [SugarTable("sys_dict")]
    public class Dict : BaseEntityNoDataScope
    {
        /// <summary>
        /// 字典类型
        /// </summary>
        /// <returns></returns>
        public DictType DictType { get; set; }

        /// <summary>
        /// 字典名称
        /// </summary>
        /// <returns></returns>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        #region 扩展属性

        /// <summary>
        /// 字典详情
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(NavigateType.OneToMany, nameof(DictDetail.DictId))]
        public List<DictDetail>? DictDetails { get; set; }

        #endregion
    }
}

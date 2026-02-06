using System.Collections.Generic;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.Permission
{
    /// <summary>
    /// 系统菜单
    /// </summary>
    [SugarTable("sys_menu")]
    public class Menu : BaseEntity
    {
        /// <summary>
        /// 菜单标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 组件路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 权限标识符
        /// </summary>
        public string? AuthCode { get; set; }

        /// <summary>
        /// 组件
        /// </summary>
        public string? Component { get; set; }

        /// <summary>
        /// 组件名称
        /// </summary>
        public string? ComponentName { get; set; }

        /// <summary>
        /// 父级菜单ID
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(DefaultValue = "999")]
        public int Sort { get; set; }

        /// <summary>
        /// icon图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 菜单类型
        /// </summary>
        public MenuType MenuType { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 是否缓存
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// 子节点个数
        /// </summary>
        [SugarColumn(DefaultValue = "0")]
        public int SubCount { get; set; }

        /// <summary>
        /// 徽章类型
        /// </summary>
        public BadgeType? BadgeType { get; set; }

        /// <summary>
        /// 徽章文字
        /// </summary>
        public string? BadgeText { get; set; }

        /// <summary>
        /// 徽章样式
        /// </summary>
        public string? BadgeStyle { get; set; }

        /// <summary>
        /// 子菜单集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<Menu>? Children { get; set; }
    }
}

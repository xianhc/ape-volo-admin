using Ape.Volo.Entity.Base;
using Ape.Volo.Shared.Enums;
using SqlSugar;

namespace Ape.Volo.Entity.Identity
{
    /// <summary>
    /// 系统菜单
    /// </summary>
    [SugarTable("sys_menu")]
    [SugarIndex("index_{table}_ParentId", nameof(ParentId), OrderByType.Asc)]
    public class Menu : DataScopedEntity
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
        /// 父级菜单ID（null 表示顶级菜单）
        /// </summary>
        public long? ParentId { get; set; }

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
        /// 子菜单集合
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<Menu>? Children { get; set; }
    }
}

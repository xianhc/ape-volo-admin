using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Permission.Menus.Models;

/// <summary>
/// 菜单Vo
/// </summary>
public class MenuVo : BaseEntityDto<long>
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 路径
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 权限标识
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
    /// 父级ID（null 表示顶级菜单）
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// Icon
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public MenuType MenuType { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 缓存
    /// </summary>
    public bool KeepAlive { get; set; }

    /// <summary>
    /// 隐藏
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public List<MenuVo>? Children { get; set; }

    /// <summary>
    /// 是否有子节点，供懒加载树表判断是否显示展开箭头
    /// </summary>
    public bool HasChildren { get; set; }
}

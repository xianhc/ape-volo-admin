namespace Ape.Volo.ViewModel.Core.Permission.Menu;

/// <summary>
/// 菜单Meta
/// </summary>
public class MenuMetaVo
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public MenuMetaVo()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="title"></param>
    /// <param name="icon"></param>
    /// <param name="keepAlive"></param>
    public MenuMetaVo(string title, string icon, bool keepAlive)
    {
        Title = title;
        Icon = icon;
        KeepAlive = keepAlive;
        CloseTab = false;
        ActiveName = "";
    }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Icon
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 缓存
    /// </summary>
    public bool KeepAlive { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool CloseTab { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string ActiveName { get; set; }
}
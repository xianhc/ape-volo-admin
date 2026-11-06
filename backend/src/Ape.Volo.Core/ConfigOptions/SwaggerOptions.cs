using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

[OptionsSettings]
public class SwaggerOptions
{
    public bool Enabled { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 联系人名称，同时用于文档描述中的用户名。
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// 联系人邮箱。
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// 官网绝对地址；留空时不显示官网链接。
    /// </summary>
    public string ContactUrl { get; set; } = string.Empty;
}

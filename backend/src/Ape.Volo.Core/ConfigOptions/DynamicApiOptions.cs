using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// 动态接口配置
/// </summary>
[OptionsSettings]
public class DynamicApiOptions
{
    /// <summary>
    /// 路由前缀，默认 api
    /// </summary>
    public string RoutePrefix { get; set; } = "api";

    /// <summary>
    /// 额外排除的远程服务接口名；未继承 IRemoteService 的接口默认不公开
    /// </summary>
    public string[] ExcludeServices { get; set; } = [];
}

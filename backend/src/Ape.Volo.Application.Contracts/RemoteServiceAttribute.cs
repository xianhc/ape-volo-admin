namespace Ape.Volo.Application.Contracts;

/// <summary>
/// 控制远程契约或方法是否公开；关闭 HTTP 暴露不影响进程内接口调用
/// </summary>
/// <param name="isEnabled">是否启用远程暴露</param>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, Inherited = false)]
public sealed class RemoteServiceAttribute(bool isEnabled = true) : Attribute
{
    /// <summary>
    /// 是否生成动态 HTTP 端点；启用仍要求契约继承 IRemoteService
    /// </summary>
    public bool IsEnabled { get; } = isEnabled;
}

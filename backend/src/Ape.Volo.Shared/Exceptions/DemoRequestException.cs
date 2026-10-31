namespace Ape.Volo.Shared.Exceptions;

/// <summary>
/// 演示模式下的写操作被拒绝。由宿主层的演示过滤器抛出，交给异常过滤器转成提示响应。
/// </summary>
public class DemoRequestException : System.Exception
{
    /// <summary>
    /// 初始化 <see cref="DemoRequestException"/> 的新实例。
    /// </summary>
    /// <param name="message">提示信息</param>
    public DemoRequestException(string message) : base(message)
    {
    }
}

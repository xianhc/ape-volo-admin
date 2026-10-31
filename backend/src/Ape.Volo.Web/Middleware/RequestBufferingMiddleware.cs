namespace Ape.Volo.Web.Middleware;

/// <summary>
/// 启用请求正文缓冲，使签名校验、参数读取与模型绑定可以重复读取同一份正文。
/// </summary>
/// <param name="next">后续请求委托</param>
public sealed class RequestBufferingMiddleware(RequestDelegate next)
{
    /// <summary>
    /// 开启正文缓冲后转交下一个中间件。
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <returns>异步任务</returns>
    public Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        return next(context);
    }
}

namespace Ape.Volo.Web.Middleware;

/// <summary>
/// 认证完成后异步加载权限，SQL 过滤器只读取请求快照。
/// </summary>
/// <param name="next">后续请求委托</param>
/// <param name="logger">日志记录器</param>
public sealed class DataScopeMiddleware(RequestDelegate next, ILogger<DataScopeMiddleware> logger)
{
    /// <summary>
    /// 在业务查询前完成权限加载，取消请求时停止处理。
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <param name="dataScope">当前请求的数据权限快照</param>
    public async Task InvokeAsync(HttpContext context, RequestDataScope dataScope)
    {
        if (dataScope.IsRestricted)
        {
            try
            {
                await dataScope.InitializeAsync(context.RequestServices.GetRequiredService<IDataScopeService>(),
                    context.RequestAborted);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "加载数据权限失败，本次请求不授予受限数据访问权。");
            }
        }

        await next(context);
    }
}

using Ape.Volo.Web.Logging;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 日志异步写入扩展配置
/// </summary>
public static class LogQueueExtensions
{
    /// <summary>
    /// 注册日志异步写入队列与后台写入服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLogQueueService(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddSingleton(typeof(ILogQueue<>), typeof(LogQueue<>));
        services.AddHostedService<OperateLogWriteService>();
        services.AddHostedService<ExceptionLogWriteService>();

        return services;
    }
}

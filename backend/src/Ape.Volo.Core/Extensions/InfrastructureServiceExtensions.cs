using Ape.Volo.Application.Contracts.Message.Email;
using Ape.Volo.Application.Contracts.SystemManagement.ServerInfo;
using Ape.Volo.Core.Message.Email;
using Ape.Volo.Core.SystemManagement.ServerInfo;
using Microsoft.Extensions.DependencyInjection;

namespace Ape.Volo.Core.Extensions;

/// <summary>
/// 基础设施适配器的显式注册，不依赖业务程序集扫描。
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// 注册 SMTP 发送与本机资源探测实现。
    /// </summary>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ISmtpClientFactory, SmtpClientFactory>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IServerResourcesService, ServerResourcesService>();
        return services;
    }
}

using Ape.Volo.Web.Imaging;
using Ape.Volo.Web.Storage;
using Ape.Volo.Application.Contracts.Abstractions.Security;
using Ape.Volo.Core.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 宿主适配器注册：HTTP 客户端信息、Web 文件存储与验证码图片生成。
/// </summary>
public static class HostServicesExtensions
{
    /// <summary>
    /// 注册宿主对应用端口的实现。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="passwordPrivateKeyPem">密码传输 PKCS#8 PEM 私钥</param>
    public static IServiceCollection AddHostServices(this IServiceCollection services, string passwordPrivateKeyPem)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IClientEnvironment, ClientEnvironment>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<ICaptchaImageGenerator, CaptchaImageGenerator>();
        services.AddScoped<IPasswordDecryptor>(_ => new RsaPasswordDecryptor(passwordPrivateKeyPem));
        return services;
    }
}

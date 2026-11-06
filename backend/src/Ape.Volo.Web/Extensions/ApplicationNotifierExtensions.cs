using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 应用启动提示扩展
/// </summary>
public static class ApplicationNotifierExtensions
{
    /// <summary>
    /// 注册启动完成回调，输出版本、访问地址等提示信息
    /// </summary>
    /// <param name="app">应用实例</param>
    /// <exception cref="ArgumentNullException">应用实例为 null</exception>
    public static void ApplicationStartedNotifier(this WebApplication app)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var port = GetListeningPort(app);

            ConsoleHelper.WriteLine("欢迎使用《ape-volo-admin》中后台权限管理系统\n" +
                                    "当前版本：v3.6.2\n" +
                                    "加群方式：微信号：apevolo<备注'加群'>   QQ群：839263566\n" +
                                    "官方网站：https://www.apevolo.com\n" +
                                    $"接口文档地址：http://localhost:{port}/swagger/api/index.html\n" +
                                    "前端运行地址：http://localhost:9001\n" +
                                    "--------------------------------------版权声明-----------------------------\n" +
                                    "** 版权所有 © xianhc。保留所有权利。 **\n" +
                                    "** 移除版权标识需购买商用授权：https://www.apevolo.com/empower/index.html **",
                ConsoleColor.Green);
        });
    }

    /// <summary>
    /// 获取实际监听端口
    /// 注：端口可能来自 launchSettings / ASPNETCORE_URLS / appsettings，读服务器绑定结果最准
    /// </summary>
    private static string GetListeningPort(WebApplication app)
    {
        var address = app.Services.GetService<IServer>()?
            .Features.Get<IServerAddressesFeature>()?
            .Addresses.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(address))
        {
            // 通配地址（* 或 +）无法直接交给 Uri 解析，替换成 localhost
            var normalized = address.Replace("://*", "://localhost").Replace("://+", "://localhost");
            if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
                return uri.Port.ToString();
        }

        return "9002";
    }
}

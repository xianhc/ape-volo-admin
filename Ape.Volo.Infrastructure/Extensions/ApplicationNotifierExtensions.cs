using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Microsoft.AspNetCore.Builder;

namespace Ape.Volo.Infrastructure.Extensions;

public static class ApplicationNotifierExtensions
{
    public static void ApplicationStartedNotifier(this WebApplication app)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var port = "8002";
            if (app.Configuration["urls"] != null)
            {
                port = app.Configuration["urls"].Split(':').Last();
            }

            ConsoleHelper.WriteLine("欢迎使用《ape-volo-admin》中后台权限管理系统\n" +
                                    "当前版本：v3.5.1\n" +
                                    "加群方式：微信号：apevolo<备注'加群'>   QQ群：839263566\n" +
                                    "官方网站：https://www.apevolo.com\n" +
                                    $"接口文档地址：http://localhost:{port}/swagger/api/index.html\n" +
                                    "前端运行地址：http://localhost:8001\n" +
                                    "--------------------------------------版权声明-----------------------------\n" +
                                    "** 版权所有方：xianhc **\n" +
                                    "** 移除版权标识需购买商用授权：https://www.apevolo.com/empower/index.html **",
                ConsoleColor.Green);
        });
    }
}
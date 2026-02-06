using Ape.Volo.Common.Enums;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Ape.Volo.Infrastructure.Middleware;

/// <summary>
/// Swagger授权访问中间件
/// </summary>
public class SwaggerAuthorizeMiddleware
{
    private readonly RequestDelegate _next;

    public SwaggerAuthorizeMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Value != null &&
            context.Request.Path.Value.ToLower().Contains("index.html"))
        {
            // 判断权限是否正确
            if (IsAuthorized(context))
            {
                await _next.Invoke(context);
                return;
            }

            // 无权限，跳转swagger登录页
            var returnUrl = context.Request.GetDisplayUrl();
            context.Response.Redirect("/swagger/login.html?returnUrl=" + returnUrl);
        }
        else
        {
            await _next.Invoke(context);
        }
    }

    public bool IsAuthorized(HttpContext context)
    {
        if (App.GetOptions<SystemOptions>().RunMode == RunMode.Dev)
        {
            //开发环境免登录
            return true;
        }

        var swaggerKey = context.Session.GetInt32("swagger-key");
        return swaggerKey is 1;
    }
}

public static class SwaggerAuthorizeExtensions
{
    public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
    {
        if (App.GetOptions<SwaggerOptions>().Enabled)
        {
            return builder.UseMiddleware<SwaggerAuthorizeMiddleware>();
        }

        return builder;
    }
}
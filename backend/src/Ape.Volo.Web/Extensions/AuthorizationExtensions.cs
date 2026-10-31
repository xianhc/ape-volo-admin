using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// JWT 认证与授权中间件注册
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// 注册 JWT 认证、自定义策略授权及相关服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddAuthorizationService(this IServiceCollection services)
    {
        if (services.IsNull()) throw new ArgumentNullException(nameof(services));


        services.AddScoped<ICurrentUser, HttpUser>();
        services.AddScoped<ITokenService, TokenService>();
        // 应用层的登录编排只需要签发端口，与宿主侧的 ITokenService 共用同一实现
        services.AddScoped<ITokenIssuer>(provider => (ITokenIssuer)provider.GetRequiredService<ITokenService>());
        services.TryAddSingleton(TimeProvider.System);

        services.AddAuthorization();

        // 开启Bearer认证
        services.AddAuthentication(o =>
            {
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = nameof(ApiResponseHandler);
                o.DefaultForbidScheme = nameof(ApiResponseHandler);
            })
            // 添加JwtBearer服务
            .AddJwtBearer()
            .AddScheme<AuthenticationSchemeOptions, ApiResponseHandler>(nameof(ApiResponseHandler), _ => { });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtAuthOptions>>((options, configuredJwt) =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = JwtTokenValidation.CreateParameters(configuredJwt.Value, true);
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var tokens = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                        if (!await tokens.ValidateAccessAsync(context.Principal!, context.HttpContext.RequestAborted))
                            context.Fail("登录会话无效或已撤销。");
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // 如果过期，把过期信息添加到头部
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }

                        return Task.CompletedTask;
                    }
                };
            });


    }
}

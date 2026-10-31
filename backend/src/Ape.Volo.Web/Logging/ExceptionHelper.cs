using Microsoft.AspNetCore.Http.Extensions;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 统一输出日志格式
/// </summary>
public static class ExceptionHelper
{
    /// <summary>
    /// 自定义返回格式
    /// </summary>
    /// <param name="httpContext">http上下文</param>
    /// <param name="remoteIp">远程IP</param>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="exception">异常类</param>
    /// <param name="currentUserName">当前用户名</param>
    /// <param name="operatingSystem">操作系统</param>
    /// <param name="deviceType">设备类型</param>
    /// <param name="browserName">浏览器名称</param>
    /// <param name="version">版本号</param>
    /// <param name="requestContent">调用方已脱敏的请求内容</param>
    /// <returns></returns>
    public static string ErrorFormat(HttpContext httpContext, string? remoteIp, string? ipAddress, Exception exception,
        string? currentUserName,
        string? operatingSystem, string? deviceType, string? browserName, string? version, string requestContent)
    {
        return string.Format("\r\n【异常信息】 : {0} \r\n" +
                             "【异常类型】 : {1} \r\n" +
                             "【请求路径】 : {2} \r\n" +
                             "【请求类型】 : {3} \r\n" +
                             "【请求内容】 : {4} \r\n" +
                             "【当前用户】 : {5} \r\n" +
                             "【当前IP】  : {6} \r\n" +
                             "【IP地址】  : {7} \r\n" +
                             "【操作系统】  : {8} \r\n" +
                             "【设备类型】  : {9} \r\n" +
                             "【浏览器】  : {10} \r\n" +
                             "【版本号】  : {11} \r\n" +
                             "【堆栈调用】 ：{12} \r\n" +
                             "【完整异常】 ：{13}", exception.Message, exception.GetType().Name,
            // Query 已由调用方读取并脱敏后放入请求内容，避免 URL 再次输出原始凭证。
            UriHelper.BuildAbsolute(httpContext.Request.Scheme, httpContext.Request.Host,
                httpContext.Request.PathBase, httpContext.Request.Path), httpContext.Request.Method,
            requestContent, currentUserName,
            remoteIp, ipAddress,
            operatingSystem, deviceType, browserName, version, exception.StackTrace,
            Common.Helper.ExceptionHelper.GetExceptionAllMsg(exception));
    }
}

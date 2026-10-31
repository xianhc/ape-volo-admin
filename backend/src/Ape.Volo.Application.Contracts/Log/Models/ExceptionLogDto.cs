using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Log.Models;

/// <summary>
/// 异常日志Dto
/// </summary>
public class ExceptionLogDto : BaseEntityDto<long>
{
    /// <summary>
    /// 创建者账号，日志列表直接展示
    /// </summary>
    public string? CreatorName { get; set; }

    /// <summary>
    /// 区域
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// 控制器
    /// </summary>
    public string? Controller { get; set; }

    /// <summary>
    /// 方法
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// 请求方式
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 请求地址
    /// </summary>
    public string? RequestUrl { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    public string? RequestParameters { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// 异常全量信息
    /// </summary>
    public string? ExceptionMessageFull { get; set; }

    /// <summary>
    /// 异常堆栈
    /// </summary>
    public string? ExceptionStack { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// 请求IP
    /// </summary>
    public string? RequestIp { get; set; }

    /// <summary>
    /// IP归属地
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// 浏览器名称
    /// </summary>
    public string? BrowserName { get; set; }

    /// <summary>
    /// 浏览器版本
    /// </summary>
    public string? Version { get; set; }
}

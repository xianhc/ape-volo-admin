

namespace Ape.Volo.Shared.WebApp;

/// <summary>
/// 在线用户
/// </summary>
public class LoginUserInfo
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 部门ID
    /// </summary>
    public long DeptId { get; set; }

    /// <summary>
    /// 用户部门
    /// </summary>
    public string DeptName { get; set; } = string.Empty;


    /// <summary>
    /// 请求IP
    /// </summary>
    public string Ip { get; set; } = string.Empty;

    /// <summary>
    /// IP详细地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 操作系统
    /// </summary>
    public string OperatingSystem { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 浏览器名称
    /// </summary>
    public string BrowserName { get; set; } = string.Empty;

    /// <summary>
    /// 浏览器版本
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 在线会话标识（保留字段名兼容前端，不包含原始令牌）
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 登录时间
    /// </summary>
    public DateTime LoginTime { get; set; }
}

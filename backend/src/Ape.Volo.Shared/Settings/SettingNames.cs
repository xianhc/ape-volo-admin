namespace Ape.Volo.Shared.Settings;

/// <summary>
/// 系统设置名称，与 sys_setting 种子数据保持一致。
/// </summary>
public static class SettingNames
{
    /// <summary>
    /// 默认系统邮件发送账户 ID。
    /// </summary>
    public const string DefaultEmailAccountId = "App.Email.DefaultAccountId";

    /// <summary>
    /// 是否将异常日志保存到数据库。
    /// </summary>
    public const string ExceptionLogSaveToDatabase = "App.Logging.ExceptionLog.SaveToDatabase";

    /// <summary>
    /// 是否将操作日志保存到数据库。
    /// </summary>
    public const string OperateLogSaveToDatabase = "App.Logging.OperateLog.SaveToDatabase";
}

using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// 邮件配置
/// </summary>
[OptionsSettings]
public class EmailOptions
{
    /// <summary>
    /// 是否校验 SMTP 服务端 SSL 证书。
    /// 注意：邮件发送已改为框架内置 System.Net.Mail，该实现不提供自定义证书校验回调，
    /// 因此无论此处取值如何都会校验服务端证书；保留该配置项仅为兼容既有 appsettings。
    /// 自签证书场景请把证书安装到系统受信任根证书存储
    /// </summary>
    public bool ValidateServerCertificate { get; set; } = true;
}

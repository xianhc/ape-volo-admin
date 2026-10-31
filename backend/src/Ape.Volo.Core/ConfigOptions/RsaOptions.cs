using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// RSA密钥配置
/// </summary>
[OptionsSettings]
public class RsaOptions
{
    /// <summary>
    /// PKCS#8 PEM 私钥文件路径；相对路径以 Web ContentRoot 为基准。
    /// </summary>
    public string PrivateKeyPath { get; set; } = string.Empty;
}

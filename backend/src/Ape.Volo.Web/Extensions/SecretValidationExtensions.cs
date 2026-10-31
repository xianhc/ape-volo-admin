using System.Security.Cryptography;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 启动期密钥校验：缺失时直接阻止启动，避免以空密钥运行。
/// </summary>
/// <remarks>
/// 密钥内容不内联到 <c>appsettings.json</c>。仓库中的 <c>.secrets/password-private.pem</c>
/// 仅用于开源项目的开发和演示；生产部署必须通过同一路径挂载各环境自己的私钥文件。
/// </remarks>
public static class SecretValidationExtensions
{
    /// <summary>
    /// 校验运行必需的密钥已通过配置、环境变量或用户机密提供。
    /// </summary>
    /// <param name="configuration">配置根</param>
    /// <param name="contentRootPath">Web 内容根目录</param>
    /// <returns>验证后的 PKCS#8 PEM 私钥内容</returns>
    /// <exception cref="InvalidOperationException">任一必需密钥缺失或长度不足</exception>
    public static string ValidateRequiredSecrets(this IConfiguration configuration, string contentRootPath)
    {
        var configuredPath = Require(configuration, "Rsa:PrivateKeyPath",
            "4096 位 PKCS#8 PEM 私钥文件，用于解密前端提交的密码密文");
        Require(configuration, "System:HmacSecret", "生成 AppSecret 用的 HMAC 密钥", minLength: 16);

        var privateKeyPath = Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath));
        var publicRoot = Path.GetFullPath(Path.Combine(contentRootPath, "wwwroot")) + Path.DirectorySeparatorChar;
        if (privateKeyPath.StartsWith(publicRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Rsa:PrivateKeyPath 不能位于 wwwroot 公开目录中。");
        }

        if (!File.Exists(privateKeyPath))
        {
            throw new InvalidOperationException($"Rsa:PrivateKeyPath 指向的文件不存在：{privateKeyPath}");
        }

        var privateKeyPem = File.ReadAllText(privateKeyPath);
        if (!privateKeyPem.Contains("-----BEGIN PRIVATE KEY-----", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("RSA 私钥必须是 PKCS#8 PEM（BEGIN PRIVATE KEY）。");
        }

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);
            if (rsa.KeySize != 4096 || rsa.ExportParameters(true).D is null)
            {
                throw new InvalidOperationException("密码传输 RSA 私钥必须为 4096 位并包含私钥参数。");
            }
        }
        catch (Exception exception) when (exception is CryptographicException or ArgumentException)
        {
            throw new InvalidOperationException("无法解析 Rsa:PrivateKeyPath 指向的 RSA 私钥文件。", exception);
        }

        return privateKeyPem;
    }

    /// <summary>
    /// 读取并校验单个密钥。
    /// </summary>
    /// <param name="configuration">配置根</param>
    /// <param name="key">配置键</param>
    /// <param name="description">用途说明，写入异常信息</param>
    /// <param name="minLength">最小长度</param>
    /// <exception cref="InvalidOperationException">缺失或过短</exception>
    private static string Require(IConfiguration configuration, string key, string description, int minLength = 1)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value) || value.Length < minLength)
        {
            throw new InvalidOperationException(
                $"缺少必需配置 {key}（{description}）。请通过 appsettings.Development.json、" +
                $"环境变量或 dotnet user-secrets 提供，长度至少 {minLength}。");
        }

        return value;
    }
}

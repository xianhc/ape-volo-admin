using System.Security.Cryptography;
using Ape.Volo.Application.Contracts.Abstractions.Security;
using Ape.Volo.Common.Helper;

namespace Ape.Volo.Core.Security;

/// <summary>
/// 使用 4096 位 PKCS#8 PEM 私钥解密 RSA-OAEP-SHA256 密码密文。
/// </summary>
public sealed class RsaPasswordDecryptor : IPasswordDecryptor, IDisposable
{
    private readonly RsaHelper _rsa;

    /// <summary>
    /// 初始化密码解密器并验证私钥规格。
    /// </summary>
    /// <param name="privateKeyPem">PKCS#8 PEM 私钥</param>
    public RsaPasswordDecryptor(string privateKeyPem)
    {
        using var validator = RSA.Create();
        validator.ImportFromPem(privateKeyPem);
        if (validator.KeySize != 4096 || validator.ExportParameters(true).D is null)
        {
            throw new CryptographicException("Password transport requires a 4096-bit RSA private key");
        }

        _rsa = new RsaHelper(privateKey: privateKeyPem);
    }

    /// <inheritdoc />
    public bool TryDecrypt(string cipherText, out string plainText) => _rsa.TryDecrypt(cipherText, out plainText);

    /// <inheritdoc />
    public void Dispose() => _rsa.Dispose();
}

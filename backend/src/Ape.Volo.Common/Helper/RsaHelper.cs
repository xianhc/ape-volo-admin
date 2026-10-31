using System.Security.Cryptography;
using System.Text;
using Ape.Volo.Common.Enums;

namespace Ape.Volo.Common.Helper;

/// <summary>
/// RSA加解密 使用OpenSSL的公钥加密/私钥解密
/// 
/// 公私钥请使用openssl生成  ssh-keygen -t rsa 命令生成的公钥私钥是不行的
/// 
/// </summary>
public sealed class RsaHelper : IDisposable
{
    private readonly RSA? _privateKeyRsaProvider;
    private readonly RSA? _publicKeyRsaProvider;
    private readonly HashAlgorithmName _hashAlgorithmName;

    #region 实例化RSAHelper

    /// <summary>
    /// 实例化RSAHelper
    /// </summary>
    /// <param name="rsaType">签名摘要类型；密码加解密固定使用 OAEP-SHA256</param>
    /// <param name="privateKey">PKCS#8 PEM 私钥</param>
    /// <param name="publicKey">SPKI PEM 公钥</param>
    public RsaHelper(string privateKey = "", string publicKey = "", RsaType rsaType = RsaType.Rsa2)
    {
        if (string.IsNullOrWhiteSpace(privateKey) && string.IsNullOrWhiteSpace(publicKey))
        {
            throw new ArgumentException("RSA private key and public key cannot both be empty");
        }

        if (!string.IsNullOrWhiteSpace(privateKey))
        {
            _privateKeyRsaProvider = CreateRsaProviderFromPrivateKey(privateKey);
        }

        if (!string.IsNullOrWhiteSpace(publicKey))
        {
            _publicKeyRsaProvider = CreateRsaProviderFromPublicKey(publicKey);
        }

        _hashAlgorithmName = rsaType == RsaType.Rsa ? HashAlgorithmName.SHA1 : HashAlgorithmName.SHA256;
    }

    #endregion

    #region 使用私钥签名

    /// <summary>
    /// 使用私钥签名
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <returns></returns>
    public string Sign(string data)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        var signatureBytes =
            _privateKeyRsaProvider!.SignData(dataBytes, _hashAlgorithmName, RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signatureBytes);
    }

    #endregion

    #region 使用公钥验证签名

    /// <summary>
    /// 使用公钥验证签名
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <param name="sign">签名</param>
    /// <returns></returns>
    public bool Verify(string data, string sign)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] signBytes = Convert.FromBase64String(sign);

        var verify =
            _publicKeyRsaProvider!.VerifyData(dataBytes, signBytes, _hashAlgorithmName, RSASignaturePadding.Pkcs1);

        return verify;
    }

    #endregion

    #region 解密

    public string Decrypt(string cipherText)
    {
        if (_privateKeyRsaProvider == null)
        {
            throw new System.Exception("_privateKeyRsaProvider is null");
        }

        var plainBytes = _privateKeyRsaProvider.Decrypt(Convert.FromBase64String(cipherText),
            RSAEncryptionPadding.OaepSHA256);
        return new UTF8Encoding(false, true).GetString(plainBytes);
    }

    /// <summary>
    /// 尝试解密。密文由调用方提供，格式非法或与密钥不匹配时返回 false，
    /// 避免把客户端输入问题抛成服务端异常。
    /// </summary>
    /// <param name="cipherText">Base64 密文</param>
    /// <param name="plainText">解密后的明文；失败时为空字符串</param>
    /// <returns>解密成功返回 true</returns>
    public bool TryDecrypt(string cipherText, out string plainText)
    {
        plainText = string.Empty;
        if (_privateKeyRsaProvider == null || string.IsNullOrWhiteSpace(cipherText))
        {
            return false;
        }

        try
        {
            plainText = Decrypt(cipherText);
            return true;
        }
        catch (System.Exception exception) when (exception is FormatException or CryptographicException
                                                     or ArgumentException)
        {
            return false;
        }
    }

    #endregion

    #region 加密

    public string Encrypt(string text)
    {
        if (_publicKeyRsaProvider == null)
        {
            throw new System.Exception("_publicKeyRsaProvider is null");
        }

        var plainBytes = Encoding.UTF8.GetBytes(text);
        var maxPlaintextLength = _publicKeyRsaProvider.KeySize / 8 - 2 * 32 - 2;
        if (plainBytes.Length > maxPlaintextLength)
        {
            throw new ArgumentOutOfRangeException(nameof(text),
                $"RSA-OAEP-SHA256 plaintext cannot exceed {maxPlaintextLength} UTF-8 bytes");
        }

        return Convert.ToBase64String(_publicKeyRsaProvider.Encrypt(plainBytes,
            RSAEncryptionPadding.OaepSHA256));
    }

    #endregion

    #region 使用私钥创建RSA实例

    public RSA? CreateRsaProviderFromPrivateKey(string privateKey)
    {
        var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(privateKey);
            if (rsa.ExportParameters(true).D is null)
            {
                throw new CryptographicException("The PEM does not contain an RSA private key");
            }

            return rsa;
        }
        catch
        {
            rsa.Dispose();
            throw;
        }
    }

    #endregion

    #region 使用公钥创建RSA实例

    public RSA? CreateRsaProviderFromPublicKey(string publicKeyString)
    {
        var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(publicKeyString);
            return rsa;
        }
        catch
        {
            rsa.Dispose();
            throw;
        }
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        _privateKeyRsaProvider?.Dispose();
        _publicKeyRsaProvider?.Dispose();
    }
}

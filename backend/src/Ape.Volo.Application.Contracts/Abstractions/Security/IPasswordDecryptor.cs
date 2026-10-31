namespace Ape.Volo.Application.Contracts.Abstractions.Security;

/// <summary>
/// 解密客户端提交的密码传输密文。
/// </summary>
public interface IPasswordDecryptor
{
    /// <summary>
    /// 尝试解密标准 Base64 编码的密码密文。
    /// </summary>
    /// <param name="cipherText">RSA-OAEP-SHA256 密文</param>
    /// <param name="plainText">解密后的原始密码；失败时为空</param>
    bool TryDecrypt(string cipherText, out string plainText);
}

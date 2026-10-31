using System.Text;

namespace Ape.Volo.Common.Extensions;

/// <summary>
/// Stream扩展
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// 将流Stream转为byte数组
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[] ReadToBytes(this Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        byte[] bytes = new byte[stream.Length];
        // ReadExactly：Stream.Read 可能只返回部分数据（CA2022）
        stream.ReadExactly(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);

        return bytes;
    }

    /// <summary>
    /// 将流读为字符串
    /// 注：默认使用UTF-8编码
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns></returns>
    public static string ReadToString(this Stream stream)
    {
        return ReadToString(stream, Encoding.UTF8);
    }

    /// <summary>
    /// 将流读为字符串
    /// 注：使用指定编码
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="encoding">指定编码</param>
    /// <returns></returns>
    public static string ReadToString(this Stream stream, Encoding encoding)
    {
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        string resStr = string.Empty;
        resStr = new StreamReader(stream, encoding).ReadToEnd();

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return resStr;
    }
}

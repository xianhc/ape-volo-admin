namespace Ape.Volo.Application.Contracts.Abstractions.Storage;

/// <summary>
/// 根据文件名解析媒体类型，未知扩展名返回通用二进制类型。
/// </summary>
public interface IFileTypeResolver
{
    /// <summary>
    /// 使用服务端媒体类型映射解析文件名，不依赖客户端声明。
    /// </summary>
    /// <param name="fileName">原始文件名</param>
    /// <returns>MIME 类型；此结果不代表文件内容鉴定。</returns>
    string GetContentType(string fileName);
}

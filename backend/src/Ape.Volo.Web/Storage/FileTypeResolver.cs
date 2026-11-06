using Microsoft.AspNetCore.StaticFiles;

namespace Ape.Volo.Web.Storage;

/// <summary>
/// 使用 ASP.NET Core 维护的扩展名映射解析文件媒体类型。
/// </summary>
public sealed class FileTypeResolver : IFileTypeResolver
{
    private readonly FileExtensionContentTypeProvider _provider = new();

    /// <inheritdoc />
    public string GetContentType(string fileName) =>
        _provider.TryGetContentType(fileName, out var contentType) ? contentType : "application/octet-stream";
}
